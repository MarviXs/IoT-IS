import { format, formatDistanceToNowStrict } from 'date-fns';
import { enUS, sk } from 'date-fns/locale';
import { useI18n } from 'vue-i18n';

const formatToLocalTime = (val: number) => {
  if (!val) return '';
  return format(val, 'dd/MM/yyyy HH:mm:ss');
};

const formatTimeToDistance = (val: number) => {
  if (!val) return '';
  let locale;
  const i18n = useI18n();
  const localeCode = i18n.locale.value;

  if (localeCode === 'sk') {
    locale = sk;
  } else {
    locale = enUS;
  }

  return formatDistanceToNowStrict(val, { addSuffix: true, locale });
};

export { formatToLocalTime, formatTimeToDistance };
