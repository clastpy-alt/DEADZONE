using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Deadzone.Data;
using UnityEngine;

namespace Deadzone.SaveSystem
{
    /// <summary>
    /// Versioned JSON save with integrity checksum, backup, near-atomic write,
    /// migration hook and corrupted-save recovery. The directory is a constructor
    /// argument so it can be unit tested without Unity runtime paths.
    /// The checksum detects accidental corruption only; it is NOT anti-cheat.
    /// </summary>
    public sealed class SaveService
    {
        public const int CurrentVersion = 1;

        public enum LoadOutcome
        {
            Loaded,
            RecoveredFromBackup,
            CreatedNew,
            ResetAfterCorruption
        }

        public sealed class LoadResult
        {
            public PlayerData Data;
            public LoadOutcome Outcome;
        }

        [Serializable]
        private sealed class Envelope
        {
            public int version;
            public string checksum;
            public string payload;
        }

        private readonly string directory;

        public SaveService(string directory)
        {
            this.directory = directory;
        }

        public string MainPath => Path.Combine(directory, "save.json");
        public string BackupPath => Path.Combine(directory, "save.bak.json");
        public string TempPath => Path.Combine(directory, "save.tmp");

        public bool HasSave() => File.Exists(MainPath) || File.Exists(BackupPath);

        public void Save(PlayerData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Directory.CreateDirectory(directory);
            data.lastSavedUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

            string payload = JsonUtility.ToJson(data);
            var envelope = new Envelope
            {
                version = CurrentVersion,
                checksum = ComputeChecksum(payload),
                payload = payload
            };

            File.WriteAllText(TempPath, JsonUtility.ToJson(envelope), Encoding.UTF8);

            // Only promote the current main file to backup if it is itself valid,
            // so a corrupted main can never overwrite a good backup.
            if (File.Exists(MainPath))
            {
                if (TryRead(MainPath, out _, out _))
                {
                    File.Copy(MainPath, BackupPath, true);
                }
                File.Delete(MainPath);
            }
            File.Move(TempPath, MainPath);
        }

        public LoadResult Load(GameConfig config)
        {
            string[] candidates = { MainPath, TempPath, BackupPath };
            bool anyFailed = false;

            foreach (string path in candidates)
            {
                if (!File.Exists(path)) continue;

                if (TryRead(path, out PlayerData data, out string error))
                {
                    LoadOutcome outcome = path == MainPath ? LoadOutcome.Loaded : LoadOutcome.RecoveredFromBackup;
                    return new LoadResult { Data = data, Outcome = outcome };
                }

                anyFailed = true;
                Debug.LogWarning($"[Save] Could not read '{Path.GetFileName(path)}': {error}");
                Quarantine(path);
            }

            return new LoadResult
            {
                Data = PlayerData.CreateNew(config),
                Outcome = anyFailed ? LoadOutcome.ResetAfterCorruption : LoadOutcome.CreatedNew
            };
        }

        public void DeleteAll()
        {
            foreach (string path in new[] { MainPath, TempPath, BackupPath })
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private static bool TryRead(string path, out PlayerData data, out string error)
        {
            data = null;
            error = null;
            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                Envelope envelope = JsonUtility.FromJson<Envelope>(json);

                if (envelope == null || string.IsNullOrEmpty(envelope.payload))
                {
                    error = "empty or invalid envelope";
                    return false;
                }
                if (envelope.version < 1 || envelope.version > CurrentVersion)
                {
                    error = $"unsupported save version {envelope.version}";
                    return false;
                }
                if (!string.Equals(envelope.checksum, ComputeChecksum(envelope.payload), StringComparison.Ordinal))
                {
                    error = "checksum mismatch";
                    return false;
                }

                PlayerData parsed = JsonUtility.FromJson<PlayerData>(envelope.payload);
                if (parsed == null)
                {
                    error = "payload could not be parsed";
                    return false;
                }

                Migrate(parsed, envelope.version);
                Sanitize(parsed);
                data = parsed;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>Upgrade data written by older versions. v1 is the baseline.</summary>
        private static void Migrate(PlayerData data, int fromVersion)
        {
            // Example for the future:
            // if (fromVersion < 2) { data.newField = defaultValue; }
        }

        private static void Sanitize(PlayerData data)
        {
            if (string.IsNullOrEmpty(data.playerId)) data.playerId = Guid.NewGuid().ToString("N");
            if (string.IsNullOrWhiteSpace(data.playerName)) data.playerName = "Commander";
            if (data.commanderLevel < 1) data.commanderLevel = 1;
            if (data.commanderExperience < 0) data.commanderExperience = 0;
            if (data.vipLevel < 0) data.vipLevel = 0;
            if (data.combatPower < 0) data.combatPower = 0;
            if (data.resources == null) data.resources = new List<ResourceAmount>();

            data.resources.RemoveAll(r => r == null);
            foreach (ResourceAmount r in data.resources)
            {
                if (r.amount < 0) r.amount = 0;
            }
        }

        private void Quarantine(string path)
        {
            try
            {
                if (path == TempPath)
                {
                    File.Delete(path);
                    return;
                }
                string stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
                string target = Path.Combine(directory, $"save.corrupt-{stamp}-{Path.GetFileNameWithoutExtension(path)}.json");
                File.Move(path, target);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Save] Quarantine failed for '{path}': {ex.Message}");
            }
        }

        private static string ComputeChecksum(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
