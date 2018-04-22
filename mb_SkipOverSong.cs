using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about = new PluginInfo();

        private string tag { get; } = "SKIP_OVER_SONG";
        private string settings_file_path;
        private bool skip_on_shuffle = true;
        private bool skip_on_autodj = true;
        private string _custom_tag = "Custom1";
        private string custom_tag
        {
            get { return _custom_tag; }
            set
            {
                _custom_tag = value;
                skip_tag_key = (MetaDataType)Enum.Parse(typeof(MetaDataType), _custom_tag);
            }
        }
        private MetaDataType skip_tag_key;
        private bool active;

        private CheckBox form_skip_on_shuffle;
        private CheckBox form_skip_on_autodj;
        private ComboBox form_custom_tag;

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "Skip Over Song (when shuffling)";
            about.Description = "Skip all or some of a song when it is played during shuffle.";
            about.Author = "neRok";
            about.TargetApplication = "";
            about.Type = PluginType.General;
            about.VersionMajor = 1;
            about.VersionMinor = 0;
            about.Revision = 1;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = 120;

            // Get and apply any user settings, overridding the defaults.
            settings_file_path = mbApiInterface.Setting_GetPersistentStoragePath() + "SkipOverSong.xml";
            var settings = new XmlDocument();
            try
            {
                settings.Load(settings_file_path);
                XmlNode node;
                node = settings.SelectSingleNode("/settings/skip_on_shuffle");
                if (node != null) skip_on_shuffle = Convert.ToBoolean(node.InnerText);
                node = settings.SelectSingleNode("/settings/skip_on_autodj");
                if (node != null) skip_on_autodj = Convert.ToBoolean(node.InnerText);
                node = settings.SelectSingleNode("/settings/custom_tag");
                if (node != null) custom_tag = node.InnerText;
            }
            catch (System.IO.FileNotFoundException)
            {
            }

            // Determine plugins active flag.
            UpdateActiveFlag();

            return about;
        }

        public bool Configure(IntPtr panelHandle)
        {

            if (panelHandle != IntPtr.Zero)
            {
                Panel configPanel = (Panel)Panel.FromHandle(panelHandle);
                Label label;
                var y_pos = 0;

                label = new Label();
                label.AutoSize = true;
                label.Location = new Point(0, y_pos);
                label.MaximumSize = new Size(450, 0);
                label.Text = "You must set one of the custom tags on the 'Tags (1)' preference page to '"+tag+"', then set this box to that custom tag number, otherwise the plugin won't work. You need to 'Define New Tag' first.";
                configPanel.Controls.AddRange(new Control[] { label });
                y_pos = label.Bottom + 8;

                label = new Label();
                label.AutoSize = true;
                label.Location = new Point(0, y_pos);
                label.Text = "Custom tag used:";
                form_custom_tag = new ComboBox();
                form_custom_tag.Bounds = new Rectangle(180, y_pos-4, 120, form_custom_tag.Height);
                for (int i = 1; i <= 16; i++)
                    form_custom_tag.Items.Add("Custom" + i.ToString());
                form_custom_tag.SelectedItem = custom_tag;
                configPanel.Controls.AddRange(new Control[] { label, form_custom_tag });
                y_pos = label.Bottom + 8;

                // TODO, settings cannot be applied this way, because MB overrides them when it closes.
                //var button_apply_settings = new Button();
                //button_apply_settings.Text = "Apply the Custom Tag For Me (requires manual restart)";
                //button_apply_settings.AutoSize = true;
                //button_apply_settings.Location = new Point(0, y_pos);
                //button_apply_settings.Click += new EventHandler(button_apply_settings_Click);
                //configPanel.Controls.AddRange(new Control[] { button_apply_settings });
                //y_pos = button_apply_settings.Bottom + 8;

                label = new Label();
                label.AutoSize = true;
                label.Location = new Point(0, y_pos);
                label.Text = "Skip songs when Shuffle is on:";
                form_skip_on_shuffle = new CheckBox();
                form_skip_on_shuffle.Checked = skip_on_shuffle;
                form_skip_on_shuffle.Location = new Point(180, y_pos-4);
                configPanel.Controls.AddRange(new Control[] { label, form_skip_on_shuffle });
                y_pos = label.Bottom + 8;

                label = new Label();
                label.AutoSize = true;
                label.Location = new Point(0, y_pos);
                label.Text = "Skip songs when AutoDJ is on:";
                form_skip_on_autodj = new CheckBox();
                form_skip_on_autodj.Checked = skip_on_autodj;
                form_skip_on_autodj.Location = new Point(180, y_pos-4);
                configPanel.Controls.AddRange(new Control[] { label, form_skip_on_autodj });
                y_pos = label.Bottom + 8;

            }

            return true;
        }

        //public void button_apply_settings_Click(object sender, EventArgs e)
        //{

        //    // First ensure the custom tag is defined.

        //    var tag_file_path = mbApiInterface.Setting_GetPersistentStoragePath() + "CustomTagConfig.xml";
        //    var CustomTagConfig = new XmlDocument();
        //    try // Try load the xml file.
        //    {
        //        CustomTagConfig.Load(tag_file_path);
        //    }
        //    catch (System.IO.FileNotFoundException)
        //    {
        //        // Create new XML document with correct root.
        //        XmlElement root = CustomTagConfig.CreateElement("CustomTags");
        //        XmlDeclaration decl = CustomTagConfig.CreateXmlDeclaration("1.0", "utf-8", null);
        //        CustomTagConfig.AppendChild(root);
        //        CustomTagConfig.InsertBefore(decl, root);
        //    }

        //    // Check if the tag is already defined, and add it to file if not.
        //    XmlNode tag_node = CustomTagConfig.SelectSingleNode("/CustomTags/Tag[@id='" + tag + "']");
        //    if (tag_node == null)
        //    {
        //        // Node for tag doesn't exist, so create an element, and save it to file.
        //        XmlElement elem = CustomTagConfig.CreateElement("Tag");
        //        elem.SetAttribute("id", tag);
        //        elem.SetAttribute("id3v23", "TXXX/" + tag);
        //        elem.SetAttribute("id3v24", "TXXX/" + tag);
        //        elem.SetAttribute("wma", tag);
        //        elem.SetAttribute("vorbisComments", tag);
        //        elem.SetAttribute("mpeg", tag);
        //        elem.SetAttribute("ape2", tag);
        //        CustomTagConfig.SelectSingleNode("/CustomTags").AppendChild(elem);

        //        CustomTagConfig.Save(tag_file_path);
        //    }

        //    // Now load the program settings, and set the custom tag.
        //    var mb_file_path = mbApiInterface.Setting_GetPersistentStoragePath() + "MusicBee3Settings.ini";
        //    var MusicBee3Settings = new XmlDocument();
        //    MusicBee3Settings.Load(mb_file_path);
        //    MusicBee3Settings.SelectSingleNode("/ApplicationSettings/Tag" + custom_tag + "Id").InnerText = tag;
        //    MusicBee3Settings.SelectSingleNode("/ApplicationSettings/Tag" + custom_tag + "Name").InnerText = tag;
        //    MusicBee3Settings.Save(mb_file_path);

        //    // Open a message box prompting the user to re-open the program.
        //    MessageBox.Show("The settings have been applied, but you must immediately close and re-open the program for them to take affect (do not hit Save or Apply)");
        //}

        public void SaveSettings()
        {
            // Get settings from the form controls.
            skip_on_shuffle = form_skip_on_shuffle.Checked;
            skip_on_autodj = form_skip_on_autodj.Checked;
            custom_tag = form_custom_tag.SelectedItem.ToString();

            // Save the settings to XML document.
            var settings = new XDocument(
                new XElement("settings",
                    new XElement("skip_on_shuffle", skip_on_shuffle.ToString()),
                    new XElement("skip_on_autodj", skip_on_autodj.ToString()),
                    new XElement("custom_tag", custom_tag)
            ));
            settings.Save(settings_file_path);

            // Update plugins active flag.
            UpdateActiveFlag();
        }

        public void Close(PluginCloseReason reason)
        {
        }

        public void Uninstall()
        {
            System.IO.File.Delete(settings_file_path);
        }

        private void UpdateActiveFlag()
        {
            if (mbApiInterface.Player_GetShuffle() == true)
                active = skip_on_shuffle;
            else if (mbApiInterface.Player_GetAutoDjEnabled() == true)
                active = skip_on_autodj;
            else
                active = false;
        }

        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {

            switch (type) // Perform some action depending on the notification type.
            {

                case NotificationType.PlayerShuffleChanged:
                    UpdateActiveFlag();
                    break;

                case NotificationType.TrackChanged:

                    // Only process track if the plugin is active.
                    if (active == false)
                        break;

                    // Get tag value, and do not skip when the tag is not set, or is a "negative" style value.
                    //string skip_tag_value2 = mbApiInterface.NowPlaying_GetFileTag(skip_tag_key);
                    string skip_tag_value = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Custom1);
                    if (skip_tag_value == null)
                        break;
                    switch (skip_tag_value.ToLower())
                    {
                        case "":
                            break;
                        case "false":
                            break;
                        case "0":
                            break;
                        case "no":
                            break;
                        default:

                            List<Tuple<TimeSpan, TimeSpan>> ParsedTimestamps;

                            // Try parse the tag to proper timestamp ranges. If the process isn't succesful, skip the entire track.
                            if (TryParseTag(skip_tag_value, out ParsedTimestamps) == false)
                            {
                                mbApiInterface.Player_PlayNextTrack();
                                break;
                            }

                            // TODO, proper methods for handling multiple timestamps needed. Currently only checks for a new start time.

                            // If the start of the range is the start of the file, begin playing the song from the second time.
                            if (ParsedTimestamps[0].Item1.TotalMilliseconds == 0)
                            {
                                mbApiInterface.Player_SetPosition(Convert.ToInt32(ParsedTimestamps[0].Item2.TotalMilliseconds));
                                break;
                            }
                            else
                            {
                                mbApiInterface.Player_PlayNextTrack();
                                break;
                            }
                    }
                    break;
            }
        }

        private string[] TimestampFormats = {
            @"m\:ss",
            @"m\:ss\.fff",
	        @"h\:mm\:ss",
            @"h\:mm\:ss\.fff",
            @"d\:hh\:mm\:ss",
            @"d\:hh\:mm\:ss\.fff",
        };

        private bool TryParseTag(string Tag, out List<Tuple<TimeSpan, TimeSpan>> ParsedTimestamps)
        {
            // Attempt to process the tag as a series of timestamps that represent parts of the song to skip.
            // If any errors are encoutered, false will be returned.
            // If all timestamps and ranges are parsed correctly, they will be added to a list, and true returned.

            ParsedTimestamps = new List<Tuple<TimeSpan, TimeSpan>> { };

            // Check that the tag has timestamp like characters.
            if (Tag.Contains(":") == false)
                return false;

            // Split the tag into individual timestamp ranges.
            string[] timestamp_ranges = Tag.Split(',');

            // Process each timestamp range.
            TimeSpan time_start, time_end;
            foreach (var timestamp_range in timestamp_ranges)
            {
                var timestamps = timestamp_range.Split(new char[] { '-' }, 2);

                // Check we have start and end timestamps.
                if (timestamps.Length != 2)
                    return false;

                // Check if the first timestamp is 'start', otherwise try parse it.
                if (timestamps[0] == "start")
                {
                    time_start = new TimeSpan(0);
                }
                else
                {
                    if (TimeSpan.TryParseExact(timestamps[0], TimestampFormats, null, out time_start) == false)
                    {
                        // If the timestamp could not be parsed, return false.
                        return false;
                    }
                }

                // Check if the first timestamp is 'end', otherwise try parse it.
                if (timestamps[1] == "end")
                {
                    time_end = new TimeSpan(0);
                }
                else
                {

                    if (TimeSpan.TryParseExact(timestamps[1], TimestampFormats, null, out time_end) == false)
                    {
                        // If the timestamp could not be parsed, return false.
                        return false;
                    }
                }

                // Append the 2 timestamps to the result.
                ParsedTimestamps.Add(Tuple.Create(time_start, time_end));

            }

            // Sort the timestamps ranges by the start/first time.
            ParsedTimestamps.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            // All timestamps and ranges successfully parsed, so return true.
            return true;

        }

    }
}
