import { createI18n } from "vue-i18n";

const DEFAULT_LOCALE = "en";
const SUPPORTED_LOCALES = ["de", "en", "fr"] as const;

type SupportedLocale = (typeof SUPPORTED_LOCALES)[number];
type LocaleMessage = Record<string, any>;

const localeModules = import.meta.glob("../i18n/*.json", { eager: true });

const messages = Object.entries(localeModules).reduce(
  (acc, [path, module]) => {
    const fileName = path.split("/").pop();
    const locale = fileName?.replace(".json", "") as
      | SupportedLocale
      | undefined;

    if (locale && SUPPORTED_LOCALES.includes(locale)) {
      acc[locale] = (module as { default: LocaleMessage }).default;
    }

    return acc;
  },
  {} as Record<SupportedLocale, LocaleMessage>,
);

function resolveInitialLocale(): SupportedLocale {
  const browserLocale = (navigator.language || DEFAULT_LOCALE)
    .toLowerCase()
    .split("-")[0] as SupportedLocale;

  return SUPPORTED_LOCALES.includes(browserLocale)
    ? browserLocale
    : DEFAULT_LOCALE;
}

export default createI18n({
  legacy: false,
  globalInjection: true,
  locale: resolveInitialLocale(),
  fallbackLocale: DEFAULT_LOCALE,
  messages,
});
