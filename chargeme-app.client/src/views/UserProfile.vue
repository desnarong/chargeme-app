<script setup>
    defineEmits(['updateSessionToken', 'updateUser', 'changeLanguage']);
</script>

<template>
    <div class="flex flex-col items-center">
        <!-- User Picture -->
        <div class="relative w-2/5 mt-10">
            <img :src="user.fImage" alt="User Picture" class="rounded-full border w-full h-auto object-cover" />
            <input type="file" accept="image/*" @change="onFileChange" class="absolute inset-0 opacity-0 cursor-pointer" />
        </div>

        <!-- User Name -->
        <h1 class="text-[20px] font-semibold mt-4">{{ user.fName }} {{ user.fLastname }}</h1>

        <!-- Balance Display -->
        <div class="w-[90%] mt-4 text-center">
            <label class="block text-lg font-semibold text-gray-700">{{ translations.USERPROFILE_BALANCE_TEXT || 'ยอดคงเหลือ' }}:</label>
            <p class="text-xl font-bold text-green-600">{{ formatBalance(user.fBalanceKwh) }} kWh</p>
        </div>

        <!-- User Details Form -->
        <div class="w-[90%] mt-4">
            <label class="block text-left w-full">{{ translations.USERPROFILE_EMAIL_TEXT }}:</label>
            <input type="text"
                   :value="user.fEmail"
                   class="w-full p-2 border border-gray-300 rounded mt-2 mb-4 focus:ring-2 focus:ring-blue-500" readonly disabled />

            <label class="block text-left w-full">{{ translations.USERPROFILE_FIRSTNAME_TEXT }}:</label>
            <input type="text"
                   id="firstname"
                   v-model="user.fName"
                   class="w-full p-2 border border-gray-300 font-semibold rounded mt-2 mb-4 focus:ring-2 focus:ring-blue-500" />

            <label class="block text-left w-full">{{ translations.USERPROFILE_LASTNAME_TEXT }}:</label>
            <input type="text"
                   id="lastname"
                   v-model="user.fLastname"
                   class="w-full p-2 border border-gray-300 font-semibold rounded mt-2 mb-4 focus:ring-2 focus:ring-blue-500" />

            <label class="block text-left w-full">{{ translations.USERPROFILE_LANGUAGE_TEXT }}:</label>
            <select v-model="user.fLanguage" @change="changeLanguage"
                    class="p-2 mt-2 mb-4 text-[14px] block w-[100%] rounded-md text-gray-600 border-gray-300 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50">
                <option v-for="(language, key) in languages" :key="key" :value="key" class=" text-gray-600">{{ language }}</option>
            </select>

            <div class="text-center mt-3">
                <button @click="updateprofile"
                        class="w-[100%] p-5 bg-blue-500 text-white text-[16px] py-2 px-4 rounded hover:bg-blue-700 hover:text-white duration-300">
                    {{ translations.USERPROFILE_BUTTON_TEXT }}
                </button>
            </div>
        </div>
    </div>
</template>

<script>
    import axios from '@/services/api.js';
    import Swal from 'sweetalert2';

    export default {
        data() {
            return {
                selectedFile: null,
                languages: null
            };
        },
        computed: {
            token() {
                return this.$store.getters.getToken;
            },
            fid() {
                return this.$store.getters.getFid;
            },
            user: {
                get() {
                    return this.$store.getters.getUser;
                },
                set(value) {
                    this.$store.dispatch('updateUser', value);
                }
            },
            translations() {
                return this.$store.getters.getTranslations;
            },
            currentlanguage: {
                get() {
                    return this.$store.getters.getCurrentlanguage || 'en';
                },
                set(value) {
                    this.$store.dispatch('updateCurrentlanguage', value);
                }
            }
        },
        methods: {
            async goBack() {
                await this.$router.push({ name: 'home' });
            },
            async onFileChange(event) {
                this.selectedFile = event.target.files[0];
                await this.uploadProfilePicture();
            },
            async fetchLanguages() {
                try {
                    const response = await axios.get(`/translations/languages`);
                    this.languages = response.data;
                } catch (error) {
                    console.error('Error fetching languages:', error);
                }
            },
            async uploadProfilePicture() {
                if (!this.selectedFile) return;

                const formData = new FormData();
                formData.append('file', this.selectedFile);

                try {
                    const response = await axios.post('/user/upload-profile-picture', formData, {
                        headers: {
                            'Content-Type': 'multipart/form-data',
                            'Authorization': `Bearer ${this.token}`
                        },
                    });
                    this.user.fImage = response.data.newProfilePictureUrl;
                    this.user = { ...this.user }; // Trigger reactivity
                    this.showUpdateAlert('อัปโหลดรูปโปรไฟล์สำเร็จ');
                } catch (error) {
                    console.error('Error uploading profile picture:', error);
                    this.showUpdateAlert('เกิดข้อผิดพลาดในการอัปโหลดรูป', 'error');
                }
            },
            async changeLanguage() {
                this.currentlanguage = this.user.fLanguage.toLowerCase(); // Sync กับ store
                this.$emit('changeLanguage', this.user.fLanguage);
                await this.fetchTranslations();
            },
            async fetchTranslations() {
                try {
                    const response = await axios.get(`/translations/${this.currentlanguage}`);
                    this.$store.dispatch('updateTranslations', response.data);
                } catch (error) {
                    console.error('Error fetching translations:', error);
                }
            },
            async updateprofile() {
                try {
                    const payload = {
                        fName: this.user.fName,
                        fLastname: this.user.fLastname,
                        language: this.user.fLanguage
                    };

                    const response = await axios.put('/user/update-profile', payload, {
                        headers: {
                            'Authorization': `Bearer ${this.token}`,
                            'Content-Type': 'application/json',
                        },
                    });
                    this.currentlanguage = this.user.fLanguage.toLowerCase(); // Sync language
                    await this.fetchTranslations();
                    this.showUpdateAlert('อัปเดตโปรไฟล์สำเร็จ');
                } catch (error) {
                    console.error("Error updating profile:", error);
                    this.showUpdateAlert('เกิดข้อผิดพลาดในการอัปเดตโปรไฟล์', 'error');
                }
            },
            showUpdateAlert(message, icon = 'success') {
                Swal.fire({
                    icon,
                    title: "โปรไฟล์",
                    text: message,
                    confirmButtonText: this.translations.HOME_POPUP_CLOSE,
                    customClass: { popup: 'swal2-custom-height' }
                });
            },
            formatBalance(balance) {
                return balance ? Number(balance).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,') : '0.00';
            }
        },
        async mounted() {
            await this.fetchLanguages();
            if (this.user.fLanguage) {
                this.currentlanguage = this.user.fLanguage.toLowerCase();
            }
            await this.fetchTranslations();
            this.$emit('update-title', this.translations.USERPROFILE_TITLE);
        },
    };
</script>

<style scoped>
    .swal2-custom-height {
        padding: 2rem 1rem !important;
        min-height: 250px;
    }
</style>