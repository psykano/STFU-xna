using System.Collections.Generic;
using System.Text;

namespace EasyConfig
{
    /// <summary>
    /// A group of settings from a configuration file.
    /// </summary>
    public class SettingsGroup
    {
        /// <summary>
        /// Gets the name of the group.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the settings found in the group.
        /// </summary>
        public Dictionary<string, Setting> Settings { get; private set; }

        internal SettingsGroup(string name)
        {
            Name = name;
            Settings = new Dictionary<string, Setting>();
        }

        internal SettingsGroup(string name, List<Setting> settings)
        {
            Name = name;
            Settings = new Dictionary<string, Setting>();

            foreach (Setting setting in settings)
                Settings.Add(setting.Name, setting);
        }

        /// <summary>
        /// Adds a setting to the group.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        public void AddSetting(string name, int value)
        {
            Setting setting = new Setting(name);
            setting.SetValue(value);
            Settings.Add(name, setting);
        }

        /// <summary>
        /// Adds a setting to the group.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        public void AddSetting(string name, float value)
        {
            Setting setting = new Setting(name);
            setting.SetValue(value);
            Settings.Add(name, setting);
        }

        /// <summary>
        /// Adds a setting to the group.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        public void AddSetting(string name, bool value)
        {
            Setting setting = new Setting(name);
            setting.SetValue(value);
            Settings.Add(name, setting);
        }

        /// <summary>
        /// Adds a setting to the group.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        public void AddSetting(string name, string value)
        {
            Setting setting = new Setting(name);
            setting.SetValue(value);
            Settings.Add(name, setting);
        }

        /// <summary>
        /// Adds a setting to the group.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The values of the setting.</param>
        public void AddSetting(string name, params int[] values)
        {
            Setting setting = new Setting(name);
            setting.SetValue(values);
            Settings.Add(name, setting);
        }

        /// <summary>
        /// Adds a setting to the group.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The values of the setting.</param>
        public void AddSetting(string name, params float[] values)
        {
            Setting setting = new Setting(name);
            setting.SetValue(values);
            Settings.Add(name, setting);
        }

        /// <summary>
        /// Adds a setting to the group.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The values of the setting.</param>
        public void AddSetting(string name, params bool[] values)
        {
            Setting setting = new Setting(name);
            setting.SetValue(values);
            Settings.Add(name, setting);
        }

        /// <summary>
        /// Adds a setting to the group.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The values of the setting.</param>
        public void AddSetting(string name, params string[] values)
        {
            Setting setting = new Setting(name);
            setting.SetValue(values);
            Settings.Add(name, setting);
        }

        /// <summary>
        /// Deletes a setting from the group.
        /// </summary>
        /// <param name="name">The name of the setting to delete.</param>
        public void DeleteSetting(string name)
        {
            Settings.Remove(name);
        }
    }
}
