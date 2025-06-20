import { createI18n } from "vue-i18n";
import en from "./locales/en.json" // <--- add this
import th from "./locales/th.json" // <--- add this

export default createI18n({
    locale: 'th', // ตั้งค่าภาษาเริ่มต้น
    fallbackLocale: 'en', // ภาษาเริ่มต้นถ้าหากไม่พบภาษาอื่น
    legacy: false, // <--- 3
    globalInjection: true,
    messages: {
        en, // <--- add this
        th // <--- add this
    },
})