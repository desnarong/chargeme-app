<template>
    <div class="flex flex-col items-center">
        <!-- Logo -->
        <div class="mb-6 mt-6">
            <img src="../assets/logo.png" alt="Logo" class="w-[100%] mx-auto" />
        </div>

        <!-- Login Form -->
        <div class="w-full max-w-sm bg-white p-6 rounded-lg ">
            <h2 class="text-2xl font-bold text-center text-blue-600 mb-4">{{ translations.LOGIN_TITLE }}</h2>

            <!-- Email or Username Input -->
            <div class="mb-4">
                <label for="email" class="block text-sm text-gray-700">{{ translations.LOGIN_EMAIL }}</label>
                <input type="text"
                       id="email"
                       v-model="email"
                       class="w-full p-2 border border-gray-300 rounded mt-1 focus:ring-2 focus:ring-blue-500">
            </div>

            <!-- Password Input -->
            <div class="mb-6">
                <label for="password" class="block text-sm text-gray-700">{{ translations.LOGIN_PASSWORD }}</label>
                <input type="password"
                       id="password"
                       v-model="password"
                       class="w-full p-2 border border-gray-300 rounded mt-1 focus:ring-2 focus:ring-blue-500">
            </div>

            <!-- Submit Button -->
            <button @click="login"
                    class="w-full bg-blue-500 text-white font-bold py-2 rounded hover:bg-blue-600 transition duration-300">
                {{ translations.LOGIN_BUTTON }}
            </button>

            <!-- Register Link -->
            <div class="mt-4 text-center">
                <a href="/register" class="text-blue-500 hover:underline">{{ translations.LOGIN_REGISTER }}</a>
            </div>
        </div>
    </div>
</template>

<script lang="js">
    import axios from '@/services/api.js';
    import Cookies from 'js-cookie'; // นำเข้า js-cookie

    export default {
        computed: {
            token: {
                get() {
                    return this.$store.getters.getToken;
                },
                set(value) {
                    this.$store.dispatch('saveToken', value);
                }
            },
            fid: {
                get() {
                    return this.$store.getters.getFid;
                },
                set(value) {
                    this.$store.dispatch('updateFid', value);
                }
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
                    return this.$store.getters.getCurrentlanguage;
                },
                set(value) {
                    this.$store.dispatch('updateCurrentlanguage', value);
                }
            },
        },
        data() {
            return {
                email: '',
                password: '',
            };
        },
        methods: {
            goBack() {
                this.$router.push({ name: 'home' });;
            },
            async login() {
                try {
                    const token = this.$store.getters.getToken;
                    const response = await axios.post('/auth/login',
                        {
                            email: this.email,
                            password: this.password
                        },
                        {
                            headers: {
                                'Authorization': `Bearer ${token}` // ส่ง token ไปใน header
                            }
                        });

                    if (response.status === 200) {
                        //console.error(response.data);
                        const user = response.data;
                        // เก็บ token ใน cookie
                        //Cookies.set('f_user', JSON.stringify(user), { expires: 365, secure: true, sameSite: 'Lax' });
                        //Cookies.set('f_id', user.fId, { expires: 365, secure: true, sameSite: 'Lax' });

                        this.user = user;//JSON.stringify(user);
                        this.fid = user.fId;

                        await this.$emit('update-user');
                        this.currentlanguage = user.fLanguage.toLowerCase();
                        await this.$emit('change-language', user.fLanguage.toLowerCase());
                        // นำทางผู้ใช้ไปยังหน้าอื่น เช่น หน้า Home
                        // ส่งเหตุการณ์เพื่ออัปเดตโทเคนเซสชัน
                        await this.$emit('update-session-token');
                        await this.$router.push({ name: 'home' });
                    }
                } catch (error) {
                    console.error('Login failed:', error);
                    alert('Invalid login credentials');
                }
            },
            async getSessionId() {
                //this.fid = Cookies.get('f_id');
                return this.fid;
            },
        },
        async mounted() {
            // ปิด LoadingSpinner เมื่อหน้า Home ถูกโหลด
            await this.$emit('stopLoading'); // ส่งสัญญาณไปยัง App.vue เพื่อปิด Loading
        },
        beforeDestroy() {
        }
    };
</script>

<style scoped>
    /* สไตล์เพิ่มเติมถ้าจำเป็น */
</style>
