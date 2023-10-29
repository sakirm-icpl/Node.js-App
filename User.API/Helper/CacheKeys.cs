using System.Collections.Generic;

namespace User.API.Helper
{
    public static class CacheKeys
    {
        public static Dictionary<string, string> CacheSamlEmailId =new Dictionary<string, string>();
        public static string Entry => "_Entry";
        public static string EntryIVP => "_EntryIVP";
        public static string EntryENT => "_EntryENT";
        public static string EntryOrgnizationConnectionDegreed => "_EntryOrgnizationConnectionDegreed";
        public static string EntryOrgnizationConnectionGSuite => "_EntryOrgnizationConnectionGSuite";
        public static string EntryOrgnizationConnectionGSuiteOYOUAT => "_EntryOrgnizationConnectionGSuiteOYOUAT";
        public static string EntryOrgnizationConnectionO365 => "_EntryOrgnizationConnectionO365";
        public static string EntryOrgnizationConnectiono365UAT => "_EntryOrgnizationConnectiono365UAT";
        public static string CallbackEntry => "_Callback";
        public static string CallbackMessage => "_CallbackMessage";
        public static string Parent => "_Parent";
        public static string Child => "_Child";
        public static string DependentMessage => "_DependentMessage";
        public static string DependentCTS => "_DependentCTS";
        public static string Ticks => "_Ticks";
        public static string CancelMsg => "_CancelMsg";
        public static string CancelTokenSource => "_CancelTokenSource";
    }
}
