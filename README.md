# Skip Over Song (MusicBee Plugin)
MusicBee plugin to skip all or some of a song when it is played during shuffle.

When the plugin is active, it checks each music file that is to be played for a specific tag (`SKIP_OVER_SONG`). Depending on the value of the tag, the plugin will skip some or all of the song.

## Setup
Once the dll has been installed to your MusicBee plugin folder (eg `C:\Program Files (x86)\MusicBee\Plugins`), you must configure the plugin in Preferences > Plugins > Skip Over Song. Currently for the plugin to work, you must define a custom tag `SKIP_OVER_SONG` on the Tags(1) page, and then point this plugin's settings to that custom tag number.

## Valid tag values
The following tag values are **case insensitive**.

Tag type | Example Values | Result
-- | ---- | ----
No tag |  | The song is not skipped.
Empty tag | ` ` | The song is not skipped.
False | `false`, `0`,  `no` | The song is not skipped.
Timestamp | `start-01:32,06:21-end` | Parts of the song are skipped, depending upon the timestamp ranges. More info below.
Any other value | `true`, `skip`, `yes`, `1-4` | The song is skipped. Tags with incorrectly formatted timestamps will be skipped by this rule.

### Timestamps
  **Note:** Currently there are limitations with skipping certain sections of a song, which means that skipping the start of a song is the only supported method.

- Timestamps are in a comma seperated (`,`) list of ranges.
- Each range must have a start and an end timestamp, seperated by a dash (`-`).
- No spaces or other characters are allowed.
- Timestamps do not have to be in order.
- The first timestamp in a range can be subsituted for the word `start`, which represents the timestamp 0hours 0minutes 0.000seconds.
- The second timestamp in a range can be subsituted for the word `end`, which represents the end of the particular music file.
- Indiviual timestamps can be of the following values;

  ```m:ss
  m:ss.fff
  h:mm:ss
  h:mm:ss.fff
  d:hh:mm:ss
  d:hh:mm:ss.fff
  ```
  where `ss` is seconds in 2-digit-exact format (including leading zero), `m` and `mm` are minutes in >=1 and 2-digit-exact format, `fff` is milliseconds, `h` and `hh` are hours in >=1 and 2-digit-exact format, and `d` is days.
