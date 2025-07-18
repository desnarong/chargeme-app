<script setup>
    // ����ͧ import ��ǹ��������
</script>

<template>
    <div class="flex">
        <!-- Sidebar -->
       
        <div class="flex-1">
            <header class="flex items-center pl-10 pr-4 pt-4 pb-2">
                <!-- ��Ѻ pl-4 �� pl-10 ��������͹��� -->
                <!-- Back Icon and Title (�Դ����) -->
                <div v-if="title.length > 0" class="flex items-center">
                    <button @click="goBack" class="text-gray-500">
                        <i class="fas fa-chevron-left text-xl"></i>
                    </button>
                    <h1 class="text-[14px] font-semibold ml-1" @click="goBack">{{ title }}</h1>
                </div>

                <!-- Language Switcher (�Դ���) -->
                <div class="flex-1 flex justify-end h-auto max-w-full">
                    <label class="inline-flex items-center cursor-pointer">
                        <label for="lang-toggle" class="mr-3 text-sm font-medium">{{ translations.THAI_LABEL_TEXT }}</label>
                        <input id="lang-toggle"
                               type="checkbox"
                               value=""
                               class="sr-only peer"
                               v-model="isEnglish"
                               @change="toggleLanguage">
                        <div class="relative w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full rtl:peer-checked:after:-translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:start-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600"></div>
                        <label for="lang-toggle" class="ml-3 text-sm font-medium">{{ translations.ENG_LABEL_TEXT }}</label>
                    </label>

                     <Sidebar v-if="Object.keys(user).length > 0" />
                </div>

                
            </header>

            <main class="">
                <!-- overflow-auto pb-[80px] -->
                <div class="wrapper pl-4 pr-4 pb-4">
                    <LoadingSpinner :isLoading="isLoading" />
                    <router-view @update-session-token="getSessionToken"
                                 @update-user="checkUserCookie"
                                 @change-language="setLanguage"
                                 @update-title="updateTitle" />
                </div>
            </main>
        </div>
    </div>
</template>

<script>
    import LoadingSpinner from './components/LoadingSpinner.vue';
    import Sidebar from "@/components/Sidebar.vue";
    import axios from '@/services/api.js';
    import Cookies from 'js-cookie';

    export default {
        components: {
            LoadingSpinner,
            Sidebar
        },
        computed: {
            token: {
                get() { return this.$store.getters.getToken; },
                set(value) { this.$store.dispatch('saveToken', value); }
            },
            fid: {
                get() { return this.$store.getters.getFid; },
                set(value) { this.$store.dispatch('updateFid', value); }
            },
            user: {
                get() { return this.$store.getters.getUser; },
                set(value) { this.$store.dispatch('updateUser', value); }
            },
            translations: {
                get() { return this.$store.getters.getTranslations; },
                set(value) { this.$store.dispatch('updateTranslations', value); }
            },
            currentlanguage: {
                get() {
                    if (!this.$store.getters.getCurrentlanguage)this.$store.dispatch('updateCurrentlanguage', 'th');
                    return this.$store.getters.getCurrentlanguage;
                },
                set(value) { this.$store.dispatch('updateCurrentlanguage', value); }
            }
        },
        data() {
            return {
                isLoading: false,
                title: '',
                isEnglish: false // ��������Ѻ toggle language
            };
        },
        methods: {
            goBack() {
                this.$router.push({ name: 'home' });
            },
            updateTitle(title) {
                this.title = title;
            },
            toggleLanguage() {
                const newLang = this.isEnglish ? 'en' : 'th';
                this.setLanguage(newLang);
            },
            async setLanguage(newLang) {
                this.currentlanguage = newLang;
                this.isEnglish = this.currentlanguage === 'en';
                await this.fetchTranslations();
            },
            async fetchTranslations() {
                const response = await axios.get(`/translations/${this.currentlanguage}`);
                this.translations = response.data;
            },
            async checkUserCookie() {
                const userCookie = Cookies.get("f_user");
                if (userCookie) {
                    this.user = JSON.parse(decodeURIComponent(userCookie));
                }
            },
            async getSessionToken() {
                try {
                    if (this.fid) {
                        const response = await axios.get(`/session/get-token/${this.fid}`);
                        if (response.status === 200) {
                            this.token = response.data.token;
                        }
                    }
                } catch (error) {
                    console.error('Error fetching token:', error);
                }
            },
            async createGuestSession() {
                try {
                    const response = await axios.post('/session/guest-session');
                    if (response.status === 200) {
                        this.fid = response.data.message;
                        await this.getSessionToken();
                    }
                } catch (error) {
                    console.error('Error creating guest session:', error);
                }
            }
        },
        async mounted() {
            const browserLanguage = navigator.language.split('-')[0].toLowerCase();
            const supportedLanguages = ['en', 'th'];
            // this.currentlanguage = this.user.fLanguage?.toLowerCase() || (supportedLanguages.includes(browserLanguage) ? browserLanguage : 'th');
            this.currentlanguage = this.user.fLanguage?.toLowerCase() ?? 'th';
            this.isEnglish = this.currentlanguage === 'en';
            await this.fetchTranslations();
            await this.getSessionToken();
            this.isLoading = false;
        },
        async created() {
            this.$router.beforeEach((to, from, next) => {
                this.isLoading = true;
                next();
            });
            if (!this.fid) {
                await this.createGuestSession();
            }
            await this.checkUserCookie();
            this.$router.afterEach(() => {
                setTimeout(() => {
                    this.isLoading = false;
                }, 1000);
            });
        }
    };
</script>

<style scoped>
    header {
        line-height: 1.5;
    }

    .dot {
        transition: transform 0.2s ease;
    }
</style>