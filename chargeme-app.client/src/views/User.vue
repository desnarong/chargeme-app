<template>
    <!-- User Menu at the bottom -->
    <div v-if="user && user !== '' && user !== null && user !== undefined" class="fixed bottom-0 left-0 w-full bg-blue-500 shadow-lg pl-4 pr-4 flex justify-between items-center">

        <div>
            <button @click="goToHome" class="px-4 py-2 text-white rounded-full hover:bg-white hover:text-green-500 transition duration-300">
                <i class="fas fa-home text-[18px]"></i>
            </button>
        </div>

        <div>
            <button @click="goToQRCode" class="px-4 py-2  text-white rounded-full hover:bg-white hover:text-green-500 transition duration-300">
                <i class="fas fa-qrcode text-[18px]"></i>
            </button>
        </div>

        <!-- Center Column: User Info with Image -->
        <div class="relative flex flex-col items-center">
            <!-- Profile Image with 50% overflow -->
            <div class="w-16 h-16">
                <img :src="userImage" alt="User Image" @click="goToProfile" class="absolute w-18 h-18 rounded-full border-2 border-white -translate-y-2/4 cursor-pointer" />
            </div>
        </div>

        <div>
            <button @click="goToTransaction" class="px-4 py-2 text-white rounded-full hover:bg-white hover:text-green-500 transition duration-300">
                <i class="fas fa-exchange-alt text-[18px]"></i>
            </button>
        </div>

        <!--<div class="d-none">
        <button @click="goToChargeStatus" class="px-4 py-2 text-white rounded-full hover:bg-white hover:text-green-500 transition duration-300">
            <i class="fas fa-tasks text-[18px]"></i>
        </button>
    </div>

    <div class="d-none">
        <button @click="goToChargeFinish" class="px-4 py-2 text-white rounded-full hover:bg-white hover:text-green-500 transition duration-300">
            <i class="fas fa-flag-checkered text-[18px]"></i>
        </button>
    </div>-->

        <div>
            <button @click="logout" class="px-4 py-2 text-white rounded-full hover:bg-white hover:text-green-500 transition duration-300">
                <i class="fas fa-sign-out-alt text-[18px]"></i>
            </button>
        </div>
    </div>
</template>

<script>
    import axios from '@/services/api.js';

    export default {
        computed: {
            token() {
                return this.$store.getters.getToken;
            },
            fid() {
                return this.$store.getters.getFid;
            },
            user() {
                return this.$store.getters.getUser;
            },
        },
        data() {
            return {
                userImage: {}, // สถานะของการโหลด
            };
        },
        methods: {
            goToHome() {
                this.$router.push({ name: 'home' });
            },
            goToTransaction() {
                this.$router.push({ name: 'trans' });
            },
            goToQRCode() {
                this.$router.push({ name: 'scanqr' });
            },
            goToProfile() {
                this.$router.push({ name: 'profile' });
            },
            goToChargeStatus() {
                this.$router.push({ name: 'status' });
            },
            goToChargeFinish() {
                this.$router.push({ name: 'finish' });
            },
            loadUserImage() {
                //userimage.png
                //console.log(this.user.fId);
                 if (this.user.fImage === undefined) {
                    this.userImage = window.location.origin + '/userimage.png';
                }
                else
                {
                    this.userImage = window.location.origin + this.user.fImage;
                }
            },
            async logout() {
                //console.log(this.user);
                try {
                    const response = await axios.post(`/auth/logout`,
                        {}, // body ของ request (ในกรณีนี้เป็นว่างเปล่าเพราะไม่ได้ส่งข้อมูลใน body)
                        {
                            headers: {
                                'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                            }
                        });
                    // ตรวจสอบว่าการเรียก API สำเร็จ
                    if (response.status === 200) {
                        const message = response.data;
                        if (message === 'success') {
                            this.$emit('remove-session-token');
                            this.$emit('user-logout');
                        }
                    } else {
                        console.error('Failed to create guest session:', response);
                    }
                } catch (error) {
                    console.error('Error creating guest session:', error);
                }
            }
        },
        async mounted() {
            this.loadUserImage();
            //console.log(this.userImage);
        },
        watch: {
            'user.fImage': {
                immediate: true, // เรียกใช้ครั้งแรกเมื่อ component ถูก mounted
                handler(newVal) {
                    if (newVal) {
                        this.loadUserImage();
                    }
                }
            }
        },
    }
</script>

<style scoped>
    /* เพิ่มสไตล์เพิ่มเติมตามที่ต้องการ */
</style>
