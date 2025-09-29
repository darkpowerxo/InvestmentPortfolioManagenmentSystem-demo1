using Microsoft.Extensions.Localization;
using System.Globalization;

namespace InvestmentPortfolioManager.API.Services
{
    public interface ILocalizationService
    {
        string GetLocalizedString(string key);
        string GetLocalizedString(string key, params object[] arguments);
        string GetCurrentCulture();
        void SetCulture(string culture);
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public LocalizationService(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
        }

        public string GetLocalizedString(string key)
        {
            return _localizer[key];
        }

        public string GetLocalizedString(string key, params object[] arguments)
        {
            return _localizer[key, arguments];
        }

        public string GetCurrentCulture()
        {
            return CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        public void SetCulture(string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }
    }

    // This class is used to reference the resource files
    public class SharedResources
    {
    }
}