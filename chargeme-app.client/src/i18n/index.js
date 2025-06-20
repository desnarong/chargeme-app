import { createI18n } from "vue-i18n";
import en from "./locales/en.json" // <--- add this
import th from "./locales/th.json" // <--- add this

export default createI18n({
    locale: 'th', // ��駤�������������
    fallbackLocale: 'en', // ����������鹶���ҡ��辺�������
    legacy: false, // <--- 3
    globalInjection: true,
    messages: {
        en, // <--- add this
        th // <--- add this
    },
})