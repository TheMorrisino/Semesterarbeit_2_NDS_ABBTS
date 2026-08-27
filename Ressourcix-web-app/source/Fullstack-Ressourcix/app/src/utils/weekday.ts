// `t` wird injiziert, damit diese Funktion eine reine Funktion ohne vue-i18n-Abhängigkeit bleibt (siehe statusMeta.ts).

export function weekdayLabels (t: (key: string) => string): string[] {
  return [
    t('weekday.sun'), t('weekday.mon'), t('weekday.tue'), t('weekday.wed'),
    t('weekday.thu'), t('weekday.fri'), t('weekday.sat'),
  ]
}
