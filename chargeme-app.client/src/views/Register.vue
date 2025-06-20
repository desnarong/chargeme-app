<template>
    <div class="flex flex-col items-center">
        <!-- Logo -->
        <div class="mb-6 mt-6" style="display:none">
            <img src="../assets/logo.png" alt="Logo" class="w-[100%] mx-auto" />
        </div>

        <div class="w-full max-w-sm bg-white p-6 rounded-lg">
            <h2 class="text-2xl font-bold text-center text-blue-600 mb-4">{{ translations.REGISTER_TITLE }}</h2>
        </div>

        <form @submit.prevent="registerUser" class="w-full max-w-sm bg-white p-6 rounded-lg">
            <div class="mb-4 d-none">
                <label class="block text-sm text-gray-700" for="username">
                    {{ translations.REGISTER_USERNAME_TEXT }}
                </label>
                <input v-model="form.username"
                       class="w-full p-2 border border-gray-300 rounded mt-1 focus:ring-2 focus:ring-blue-500"
                       id="username"
                       type="text"
                       required />
            </div>

            <div class="mb-4">
                <label class="block text-sm text-gray-700" for="email">
                    {{ translations.REGISTER_EMAIL_TEXT }}
                </label>
                <input v-model="form.email"
                       class="w-full p-2 border border-gray-300 rounded mt-1 focus:ring-2 focus:ring-blue-500"
                       id="email"
                       type="email"
                       required />
            </div>

            <div class="mb-4">
                <label class="block text-sm text-gray-700" for="password">
                    {{ translations.REGISTER_PASSWORD_TEXT }}
                </label>
                <input v-model="form.password"
                       class="w-full p-2 border border-gray-300 rounded mt-1 focus:ring-2 focus:ring-blue-500"
                       id="password"
                       type="password"
                       required />
            </div>

            <div class="mb-4">
                <label class="block text-sm text-gray-700" for="confirmPassword">
                    {{ translations.REGISTER_CONFIRMPASSWORD_TEXT }}
                </label>
                <input v-model="form.confirmPassword"
                       class="w-full p-2 border border-gray-300 rounded mt-1 focus:ring-2 focus:ring-blue-500"
                       id="confirmPassword"
                       type="password"
                       required />
            </div>

            <div class="mb-6 text-red-500" v-if="errorMessage">
                {{ errorMessage }}
            </div>

            <div class="flex items-center justify-between">
                <button class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline"
                        type="submit">
                    {{ translations.REGISTER_TITLE }}
                </button>
            </div>
        </form>

        <p class="text-center text-gray-500 text-xs">
            {{ translations.REGISTER_ALREADY_TEXT }}
            <router-link to="/login" class="text-blue-500 hover:underline">{{ translations.LOGIN_TITLE }}</router-link>
        </p>

    </div>
</template>

<script>
    import axios from '@/services/api.js';
    import Cookies from 'js-cookie'; // นำเข้า js-cookie

    export default {
        computed: {
            token() {
                return this.$store.getters.getToken;
            },
            fid: {
                get() {
                    return this.$store.getters.getFid;
                },
                set(value) {
                    this.$store.dispatch('updateFid', value);
                }
            },
            user() {
                return this.$store.getters.getUser;
            },
            translations() {
                return this.$store.getters.getTranslations;
            },
            currentLanguage() {
                return this.$store.getters.getCurrentlanguage;
            }
        },
        data() {
            return {
                form: {
                    username: '',
                    email: '',
                    password: '',
                    confirmPassword: '',
                },
                errorMessage: '', // แสดงข้อความแจ้งเตือนกรณีเกิดข้อผิดพลาด
            };
        },
        methods: {
            async registerUser() {
                try {
                    if (this.form.password !== this.form.confirmPassword) {
                        this.errorMessage = 'รหัสผ่านไม่ตรงกัน!';
                        return;
                    }

                    // ส่งข้อมูลการลงทะเบียนไปที่ API
                    const token = this.$store.getters.getToken;
                    const response = await axios.post('/auth/register', {
                        email: this.form.email,
                        password: this.form.password,
                        username: this.form.username
                    },
                        {
                            headers: {
                                'Authorization': `Bearer ${token}` // ส่ง token ไปใน header
                            }
                        });

                    if (response.status === 200) {
                        // หากลงทะเบียนสำเร็จ ไปที่หน้า Login
                        const user = response.data;
                        //Cookies.set('f_id', user.fId, { expires: 365, secure: true, sameSite: 'Lax' });
                        this.fid = user.fId;
                        this.$router.push({ name: 'login' });
                    }
                } catch (error) {
                    if (error.response && error.response.status === 409) {
                        this.errorMessage = 'อีเมลนี้ลงทะเบียนแล้ว';
                    } else {
                        this.errorMessage = 'เกิดข้อผิดพลาดในการลงทะเบียน';
                    }
                }
            },
        },
    };
</script>

<style scoped>
    .register-container {
        padding: 20px;
        border-radius: 8px;
    }

    input:focus {
        border-color: #63b3ed;
    }
</style>
