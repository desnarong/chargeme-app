<template>
    <div class="flex justify-end h-auto max-w-full">
        <label class="inline-flex items-center cursor-pointer">
            <label for="lang-toggle" class="mr-2 text-[12px] font-medium text-gray-900 dark:text-gray-300">{{ translations.THAI_LABEL_TEXT }}</label>
            <input id="lang-toggle"
                   type="checkbox"
                   value=""
                   class="sr-only peer"
                   v-model="isEnglish"
                   @change="toggleLanguage">
            <div class="relative w-9 h-5 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 after:content-[''] after:absolute after:top-[2px] after:start-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all dark:border-gray-600"></div>
            <label for="lang-toggle" class="ml-2 text-[12px] font-medium text-gray-900 dark:text-gray-300">{{ translations.ENG_LABEL_TEXT }}</label>
        </label>
    </div>
</template>

<script>
    import { mapState, mapMutations } from 'vuex';
    export default {
        data() {
            return {
                isEnglish: false
            };
        },
        computed: {
            ...mapState({
                selectedLanguage() {
                    this.changeLanguage();
                    return this.$store.state.language === 'en';
                }
            }),
            fid() {
                return this.$store.getters.getFid;
            },
            translations() {
                return this.$store.getters.getTranslations;
            },
            currentlanguage: {
                get() {
                    return this.$store.getters.getCurrentlanguage;
                },
                set(value) {
                    this.$store.dispatch('updateCurrentlanguage', value);
                }
            },
        },
        methods: {
            toggleLanguage() {
                const newLang = this.isEnglish ? 'en' : 'th';
                this.$emit('language-changed', newLang);
                this.currentlanguage = newLang;
            },
            changeLanguage() {
                this.isEnglish = this.currentlanguage === 'en';
            }
        },
        async mounted() {
            if (!this.currentlanguage) {
                this.currentlanguage = 'th';
            }
            this.isEnglish = this.currentlanguage === 'en';
            console.log(this.currentlanguage);
        },
    };
</script>

<style scoped>
    .dot {
        transition: transform 0.2s ease;
    }
</style>