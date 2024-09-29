using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NonsensicalKit.Tools
{
    public static class TimeTool
    {
        public static TimeZoneInfo GetChineseTimeZone()
        {
            return TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        }

        public static string GetBeiJingTime(string format = "yyyy/MM/dd HH:mm:ss ddd")
        {
            return DateTime.UtcNow.AddHours(8).ToString(format);
        }

        public static string GetBeiJingTime12()
        {
            return GetBeiJingTime("yyyy/MM/dd hh:mm:ss tt ddd");
        }

        public static string FormatTips =
@"yy 年份后两位
yyyy 年份
MM 月份
dd 日数
ddd 周几
dddd 星期几
HH 小时数
mm 分钟数
ss 秒数
ff 毫秒数(最多四位)
分隔符可使用 - 或 / 或 :";
        public static string FormatTipsShort = "yy 年份后两位 yyyy 年份 MM 月份 dd 日数 ddd 周几 dddd 星期几 HH 小时数 mm 分钟数 ss 秒数 ff 毫秒数(最多四位) 分隔符可使用 - 或 / 或 :";

        public enum TimeZones
        {
            UTCm12h,
            UTCm11h,
            UTCm10h,
            UTCm9h,
            UTCm9h30m,
            UTCm8h,
            UTCm7h,
            UTCm6h,
            UTCm5h,
            UTCm4h,
            UTCm3h30m,
            UTCm3h,
            UTCm2h,
            UTCm1h,
            UTC,
            UTCp1h,
            UTCp2h,
            UTCp3h,
            UTCp3h30m,
            UTCp4h,
            UTCp4h30m,
            UTCp5h,
            UTCp5h30m,
            UTCp5h45m,
            UTCp6h,
            UTCp6h30m,
            UTCp7h,
            UTCp8h,
            UTCp9h,
            UTCp9h30m,
            UTCp10h,
            UTCp10h30m,
            UTCp11h,
            UTCp12h,
            UTCp12h45m,
            UTCp13h,
            UTCp14h,
        };

        public static List<string> TimeZoneNames = new List<string>()
        {
            "UTC-12",
            "UTC-11",
            "UTC-10",
            "UTC-9",
            "UTC-9:30",
            "UTC-8",
            "UTC-7",
            "UTC-6",
            "UTC-5",
            "UTC-4",
            "UTC-3:30",
            "UTC-3",
            "UTC-2",
            "UTC-1",
            "UTC",
            "UTC+1",
            "UTC+2",
            "UTC+3",
            "UTC+3:30",
            "UTC+4",
            "UTC+4:30",
            "UTC+5",
            "UTC+5:30",
            "UTC+5:45",
            "UTC+6",
            "UTC+6:30",
            "UTC+7",
            "UTC+8",
            "UTC+9",
            "UTC+9:30",
            "UTC+10",
            "UTC+10:30",
            "UTC+11",
            "UTC+12",
            "UTC+12:45",
            "UTC+13",
            "UTC+14",
        };

        public static List<float> TimeZoneOffsetHours = new List<float>()
        {
            -12,
            -11,
            -10,
            -9,
            -9.5f,
            -8,
            -7,
            -6,
            -5,
            -4,
            -3.5f,
            -3,
            -2,
            -1,
            0,
            1,
            2,
            3,
            3.5f,
            4,
            4.5f,
            5,
            5.5f,
            5.75f,
            6,
            6.5f,
            7,
            8,
            9,
            9.5f,
            10,
            10.5f,
            11,
            12,
            12.75f,
            13,
            14,
        };
    }
}