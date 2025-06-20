<template>
    <div class="flex flex-col items-center">
        <!-- Logo -->
        <div class="mb-6 mt-6">
            <img src="../assets/logo.png" alt="Logo" class="w-[100%] mx-auto" />
        </div>

        <!-- QR Code Image with Text -->
        <div class="text-center mb-8 mt-6">
            <img src="../assets/qrcode.png"
                 alt="QR Code"
                 class="w-[35%] mx-auto cursor-pointer"
                 @click="goToQRScan" />
            <p class="mt-2">{{ translations.SCAN_BUTTON_TEXT }}</p>
        </div>

        <!-- Search Section -->
        <div class="w-full mb-6">
            <label class="block text-left w-full">
                {{ translations.SEARCH_LABEL_TEXT }}
            </label>
            <input type="text"
                   class="w-[100%] mt-2 p-2 border border-gray-300 p-2 text-center rounded shadow-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                   @keyup.enter="performSearch"
                   v-model="searchQuery"
                   maxlength="8" />
            <button class="w-[100%] bg-blue-500 text-white py-2 rounded-md mt-4"
                    @click="performSearch">
                {{ translations.SEARCH_BUTTON_TEXT }}
            </button>
        </div>

        <!-- Links to Login and Register -->
        <div v-if="Object.keys(user).length <= 0" class="w-full text-center mt-6">
            <a @click="goToLogin" class="text-blue-500 mr-4">
                {{ translations.LOGIN_LINK_TEXT }}
            </a>
            <span class="mr-4">/</span>
            <a @click="goToRegister" class="text-blue-500">
                {{ translations.REGISTER_LINK_TEXT }}
            </a>
        </div>
        <!--<div v-if="Object.keys(user).length > 0" class="w-full text-center mt-6">
            <a @click="goToUserProfile" class="text-blue-500 mr-4">
                {{ translations.HOME_USERPROFILE_BUTTON_TEXT }}
            </a>
        </div>-->
    </div>
</template>

<script>
    import axios from '@/services/api.js';
    import Swal from 'sweetalert2';

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
            translations() {
                return this.$store.getters.getTranslations;
            },
            currentLanguage() {
                return this.$store.getters.getCurrentlanguage;
            },
            chargedata: {
                get() {
                    return this.$store.getters.getChargedata;
                },
                set(value) {
                    this.$store.dispatch('updateChargedata', value);
                }
            },
            paydata: {
                get() {
                    return this.$store.getters.getPaydata;
                },
                set(value) {
                    this.$store.dispatch('updatePaydata', value);
                }
            },
            transdata: {
                get() {
                    return this.$store.getters.getTransdata;
                },
                set(value) {
                    this.$store.dispatch('updateTransdata', value);
                }
            },
        },
        data() {
            return {
                searchQuery: null,
            };
        },
        methods: {
            goToQRScan() {
                // ใช้ Vue Router เพื่อไปหน้า ScanQRcode.vue
                this.$router.push({ name: 'scanqr' });
            },
            goToLogin() {
                // ใช้ Vue Router เพื่อไปหน้า Login.vue
                this.$router.push({ name: 'login' });
            },
            goToUserProfile() {
                // ใช้ Vue Router เพื่อไปหน้า UserProfile.vue
                this.$router.push({ name: 'profile' });
            },
            goToRegister() {
                // ใช้ Vue Router เพื่อไปหน้า Login.vue
                this.$router.push({ name: 'register' });
            },
            async performSearch() {
                if (this.searchQuery) {
                    //http://termfi.online/aa78z
                    await this.findCharger(this.searchQuery);
                }
            },
            async findCharger(charge) {
                await this.$router.push({ name: 'station', params: { id: charge } });
                //try {
                //    // เรียก API โดยใส่ token ใน header
                //    const response = await axios.post(`/api/charger/check`,
                //        {
                //            chargeid: charge
                //        },
                //        {
                //            headers: {
                //                'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                //            }
                //        });
                //    // ตรวจสอบว่าการเรียก API สำเร็จ
                //    if (response.status === 200) {
                //        if (response.data.data) {
                //            const data = response.data;
                //            this.chargedata = data.data;
                //            this.paydata = data.pay;
                //            this.transdata = data.trans;
                //            //console.log(data.data);
                //            if (this.paydata.data) {
                //                if (data.pay.data.status == 'Pending') {
                //                    await this.$router.push({ name: 'qrcode' });
                //                }
                //                else if (data.pay.data.status == 'Paid' && data.trans && (data.trans.fTransactionStatus == 'Pending' || data.trans.fTransactionStatus == 'Wating')) {
                //                    await this.$router.push({ name: 'status' });
                //                }
                //                else if (data.pay.data.status == 'Paid' && data.trans && data.trans.fTransactionStatus == 'Charging') {
                //                    await this.$router.push({ name: 'status' });
                //                }
                //                else {
                //                    // ใช้ Vue Router เพื่อไปหน้า Station.vue
                //                    await this.$router.push({ name: 'chargestation' });
                //                }
                //            } else {
                //                await this.$router.push({ name: 'chargestation' });
                //            }
                //        }
                //        else {
                //            this.showNoDataAlert();
                //        }
                //    } else {
                //        console.error('Failed to create guest session:', response);
                //    }
                //} catch (error) {
                //    console.error('Error creating guest session:', error);
                //}
            },
            showNoDataAlert() {
                Swal.fire({
                    icon: 'error',      // ไอคอนแสดงสถานะ (error, warning, info, success)
                    title: this.translations.OTHER_ERROR_TITLE,
                    text: this.translations.HOME_ERROR_TEXT, // ข้อความเพิ่มเติม
                    confirmButtonText: this.translations.HOME_POPUP_CLOSE,  // ปุ่มยืนยัน
                    customClass: {
                        popup: 'swal2-custom-height', // เพิ่มคลาสแบบกำหนดเอง
                        confirmButton: 'swal2-confirm-red', // เพิ่มคลาสสำหรับปุ่ม confirm
                    }
                });
            }
        },
        async mounted() {
            // ปิด LoadingSpinner เมื่อหน้า Home ถูกโหลด
            //await this.$emit('stopLoading'); // ส่งสัญญาณไปยัง App.vue เพื่อปิด Loading
            this.$store.dispatch('updateChargedata', {});
            this.$emit('update-title', "");
        },
        watch: {
        },
    };
</script>

<style>
    /* คุณสามารถเพิ่มสไตล์เพิ่มเติมได้ที่นี่ */
    .swal2-custom-height {
        padding: 2rem 1rem !important; /* เพิ่ม padding เพื่อเพิ่มความสูง */
        min-height: 250px; /* กำหนดความสูงขั้นต่ำ */
    }
    .swal2-confirm-red {
        background-color: #ff6666 !important; /* ตั้งค่าพื้นหลังเป็นสีแดง */
        color: white !important; /* ตั้งค่าสีตัวอักษรเป็นสีขาว */
        padding: 10px 50px !important; /* เพิ่ม padding ให้ปุ่มใหญ่ขึ้น */
    }
</style>
