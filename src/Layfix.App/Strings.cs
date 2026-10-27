namespace Layfix.App;

/// <summary>All end-user copy is keyed; layout direction follows the selected locale.</summary>
public static class Strings
{
    private static readonly Dictionary<string, (string En, string Fa)> Values = new()
    {
        ["Settings"] = ("Settings", "تنظیمات"), ["General"] = ("General", "عمومی"),
        ["Appearance"] = ("Appearance", "ظاهر"), ["Widget"] = ("Widget", "ابزارک"),
        ["Keyboard"] = ("Keyboard", "صفحه‌کلید"), ["About"] = ("About", "درباره"),
        ["EnableWidget"] = ("Enable language widget", "فعال کردن ابزارک زبان"),
        ["ShowWidget"] = ("Show widget", "نمایش ابزارک"), ["HideWidget"] = ("Hide widget", "پنهان کردن ابزارک"),
        ["Pause"] = ("Pause language observation", "توقف مشاهدهٔ زبان"), ["Resume"] = ("Resume language observation", "ادامهٔ مشاهدهٔ زبان"),
        ["Exit"] = ("Exit LAYFIX", "خروج از LAYFIX"), ["RestoreDefaults"] = ("Restore basic defaults", "بازگرداندن تنظیمات پیش‌فرض"),
        ["RestoreColors"] = ("Use Windows colors", "استفاده از رنگ‌های ویندوز"),
        ["Theme"] = ("Theme", "پوسته"), ["System"] = ("System", "سیستم"), ["Light"] = ("Light", "روشن"), ["Dark"] = ("Dark", "تیره"),
        ["FollowAccent"] = ("Follow Windows accent color", "پیروی از رنگ تأکیدی ویندوز"),
        ["CustomAccent"] = ("Custom accent color", "رنگ تأکیدی دلخواه"),
        ["Opacity"] = ("Widget opacity", "شفافیت ابزارک"), ["Size"] = ("Widget size", "اندازهٔ ابزارک"),
        ["Glass"] = ("Glass effect", "جلوهٔ شیشه‌ای"), ["Opaque"] = ("Opaque", "مات"),
        ["GlassHelp"] = ("Uses a supported Windows backdrop when available. High contrast and disabled transparency use a solid surface.", "در صورت پشتیبانی، از پس‌زمینهٔ ویندوز استفاده می‌شود. در حالت کنتراست بالا یا غیرفعال بودن شفافیت، سطح مات نمایش داده می‌شود."),
        ["LockDrag"] = ("Lock widget position", "قفل کردن موقعیت ابزارک"),
        ["Topmost"] = ("Keep widget on top", "همیشه در بالا"), ["ResetPosition"] = ("Reset widget position", "بازنشانی موقعیت ابزارک"),
        ["DragHelp"] = ("Drag the pill to move it. Click to switch the original input field. Restore it anytime from the tray.", "برای جابه‌جایی، ابزارک را بکشید. برای تغییر زبانِ کادر ورودی اصلی، کلیک کنید. بازیابی از سینی سیستم همیشه ممکن است."),
        ["UILanguage"] = ("Interface language", "زبان رابط کاربری"),
        ["ActiveLanguage"] = ("Detected foreground language", "زبان شناسایی‌شدهٔ برنامهٔ فعال"),
        ["InstalledLayouts"] = ("Loaded Windows keyboard layouts", "چیدمان‌های بارگذاری‌شدهٔ ویندوز"),
        ["EnglishTarget"] = ("English target layout", "چیدمان مقصد انگلیسی"), ["PersianTarget"] = ("Persian target layout", "چیدمان مقصد فارسی"),
        ["Automatic"] = ("Automatic", "خودکار"), ["RefreshLayouts"] = ("Refresh layouts", "تازه‌سازی چیدمان‌ها"),
        ["LastSwitch"] = ("Latest switch outcome", "نتیجهٔ آخرین تغییر زبان"),
        ["KeyboardHelp"] = ("Only verified thread layouts are shown. Some TSF profiles, elevated apps, secure desktops, and remote sessions are unsupported. A successful request alone is never confirmation.", "فقط چیدمان تأییدشدهٔ ریسه نمایش داده می‌شود. برخی پروفایل‌های TSF، برنامه‌های دارای دسترسی بالاتر، دسکتاپ امن و نشست راه دور پشتیبانی نمی‌شوند. ارسال درخواست به‌تنهایی تأیید موفقیت نیست."),
        ["Privacy"] = ("Works offline. No typed text, keystrokes, clipboard contents, window titles, or telemetry are collected. Settings stay on this device.", "به‌صورت آفلاین کار می‌کند. متن تایپ‌شده، کلیدها، محتوای کلیپ‌بورد، عنوان پنجره‌ها یا دادهٔ تله‌متری جمع‌آوری نمی‌شود. تنظیمات روی همین دستگاه می‌ماند."),
        ["Preview"] = ("Unsigned Phase 1 preview", "پیش‌نمایش بدون امضای فاز ۱"),
        ["Repository"] = ("Open repository", "باز کردن مخزن"),
        ["ResidentHelp"] = ("Closing Settings keeps LAYFIX in the tray. Use Exit LAYFIX to quit. Launch at login is deferred to a later phase.", "بستن تنظیمات، LAYFIX را در سینی سیستم نگه می‌دارد. برای پایان کار از «خروج از LAYFIX» استفاده کنید. اجرای خودکار هنگام ورود به فاز بعد موکول شده است."),
        ["Available.English"] = ("English", "انگلیسی"), ["Available.Persian"] = ("Persian", "فارسی"),
        ["Unknown"] = ("Unknown — target layout could not be verified", "نامشخص — چیدمان مقصد تأیید نشد"),
        ["Unsupported"] = ("Unsupported input target", "مقصد ورودی پشتیبانی نمی‌شود"),
        ["Unavailable"] = ("No available input target", "مقصد ورودی در دسترس نیست"), ["Paused"] = ("Observation paused", "مشاهده متوقف است"),
        ["Switch.None"] = ("No switch requested", "هنوز درخواستی ارسال نشده است"),
        ["Switch.Pending"] = ("Verifying target language…", "در حال تأیید زبان مقصد…"),
        ["Switch.Succeeded"] = ("Target thread language verified", "زبان ریسهٔ مقصد تأیید شد"),
        ["Switch.Unavailable"] = ("Target or requested language is unavailable", "مقصد یا زبان درخواستی در دسترس نیست"),
        ["Switch.Unsupported"] = ("This input target is unsupported", "این مقصد ورودی پشتیبانی نمی‌شود"),
        ["Switch.StaleContext"] = ("Focus changed; switch cancelled", "تمرکز تغییر کرد؛ درخواست لغو شد"),
        ["Switch.Blocked"] = ("Windows blocked the switch", "ویندوز درخواست تغییر زبان را مسدود کرد"),
        ["Switch.TimedOut"] = ("Language change could not be verified", "تغییر زبان تأیید نشد"),
        ["Switch.Busy"] = ("A switch is already in progress", "درخواست دیگری در حال انجام است"),
        ["Switch.Cancelled"] = ("Switch cancelled", "درخواست تغییر زبان لغو شد"),
        ["Notice.None"] = ("Changes save automatically", "تغییرات خودکار ذخیره می‌شوند"),
        ["Notice.Recovered"] = ("Settings were damaged. Safe defaults loaded.", "تنظیمات آسیب دیده بود. مقادیر پیش‌فرض امن بارگذاری شد."),
        ["Notice.ReadFailed"] = ("Settings could not be read. Safe defaults loaded.", "خواندن تنظیمات ممکن نبود. مقادیر پیش‌فرض امن بارگذاری شد."),
        ["Notice.WriteFailed"] = ("Settings could not be saved. Check access to the local app data folder.", "ذخیرهٔ تنظیمات ممکن نبود. دسترسی به پوشهٔ دادهٔ محلی برنامه را بررسی کنید."),
        ["Notice.NewerVersion"] = ("A newer settings version exists. This preview will not overwrite it.", "نسخهٔ جدیدتری از تنظیمات وجود دارد. این پیش‌نمایش آن را بازنویسی نمی‌کند."),
        ["SwitchAction"] = ("Switch English / Persian", "تغییر زبان انگلیسی / فارسی"),
        ["NonActivateFailed"] = ("Widget hidden: Windows could not enable focus protection. Use the tray to reopen Settings.", "ابزارک پنهان شد: ویندوز نتوانست حفاظت تمرکز را فعال کند. تنظیمات را از سینی سیستم باز کنید.")
    };
    public static string Get(string key, string locale) => Values.TryGetValue(key, out var value) ? locale == "fa" ? value.Fa : value.En : key;
}
