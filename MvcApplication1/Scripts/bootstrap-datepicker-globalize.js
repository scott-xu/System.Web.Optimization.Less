(function ($) {
    if (window.Globalize) {
        Globalize.culture($("html").attr("lang"));
        var culture = Globalize.culture();
        culture.calendar.patterns.g = culture.calendar.patterns.d + " " + culture.calendar.patterns.t;
        culture.calendar.patterns.G = culture.calendar.patterns.d + " " + culture.calendar.patterns.T;
        var _format = Globalize.format;
        Globalize.format = function (value, format, cultureSelector) {
            if (value == null) {
                return "";
            }
            else if (typeof value === "string") {
                return _format.apply(Globalize, [Globalize.parseDate(value.substring(0, value.indexOf("."))), "G"]) || "";
            }
            return _format.apply(Globalize, arguments);
        };
        $.fn.datepicker.dates[culture.name] = {
            days: culture.calendar.days.names,
            daysShort: culture.calendar.days.namesAbbr,
            daysMin: culture.calendar.days.namesShort,
            months: culture.calendar.months.names,
            monthsShort: culture.calendar.months.namesAbbr,
            today: "Today",
            format: "d"
        }
        $.extend($.fn.datepicker.defaults, {
            language: culture.name
        });
        $.extend($.fn.datepicker.DPGlobal, {
            parseFormat: function (format) {
                return format;
            },
            formatDate: function (date, format) {
                return Globalize.format(date, format);
            },
            parseDate: function (date, format) {
                return date ? Globalize.parseDate(date, format) : new Date();
            }
        });
    }
}(window.jQuery))