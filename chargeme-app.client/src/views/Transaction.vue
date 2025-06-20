<template>
    <div class="flex flex-col items-center">
        <!-- Top bar with Back icon and Title -->
        <!--<div class="w-full flex items-center">-->
            <!-- Back Icon -->
            <!--<button @click="goBack" class="text-gray-500">
                <i class="fas fa-chevron-left text-xl"></i>
            </button>-->
            <!-- Title -->
            <!--<h1 class="text-lg font-semibold ml-2" @click="goBack">{{ translations.TRANSACTION_TITLE }}</h1>
        </div>-->
        <!-- User Picture -->
        <div class="relative w-1/5">
            <img :src="user.fImage" alt="User Picture" class="rounded-full border w-full h-auto object-cover" />
        </div>

        <!-- User Name -->
        <h1 class="text-[18px] font-semibold mt-4">{{ user.fName }} {{ user.fLastname }}</h1>

        <div v-for="(data, index) in trans" class="grid grid-cols-6 items-center mt-5">

            <!-- คอลัมน์ที่ 1: โลโก้ -->
            <div class="col-span-1 ml-3">
                <img :src="'data:image/jpg;base64,' + data.logo" alt="Logo" class="w-[80%]">
            </div>

            <!-- คอลัมน์ที่ 2: ข้อความ -->
            <div class="col-span-3 ml-3">
                <p class="text-[12px] text-blue-400">{{ data.datetime }}</p>
                <p class="text-md text-blue-600">{{ data.stationname }}</p>
                <p class="text-[12px] text-blue-400">{{ data.headname }}</p>
            </div>
            <!-- คอลัมน์ที่ 3: ข้อความ -->
            <div class="col-span-2 ml-3">
                <p class="text-[12px] text-blue-400">พลังงาน {{ data.meterrate }} kWh</p>
                <p class="text-md text-blue-600">{{ data.cost }} บาท</p>
                <p class="text-[12px] text-blue-400">ระยะเวลา {{ data.hour }} ชั่วโมง {{ data.minute }} นาที</p>
            </div>
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
            translations() {
                return this.$store.getters.getTranslations;
            },
            currentLanguage() {
                return this.$store.getters.getCurrentlanguage;
            },
        },
        data() {
            return {
                trans: [],
            };
        },
        methods: {
            async goBack() {
                await this.$router.push({ name: 'home' });
            },
            async getTransactions() {
                try {
                    // เรียก API โดยใส่ token ใน header
                    const response = await axios.post(`/transaction/trans-list`,
                        {},
                        {
                            headers: {
                                'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                            }
                        });
                    // ตรวจสอบว่าการเรียก API สำเร็จ
                    if (response.status === 200) {
                        this.trans = response.data.data;
                    } else {
                        console.error('Failed to create guest session:', response);
                    }
                } catch (error) {
                    console.error('Error creating guest session:', error);
                }
            },
        },
        async mounted() {
            this.$emit('update-title', this.translations.TRANSACTION_TITLE);
            await this.getTransactions();
        },
    };
</script>

<style scoped>
    /* ปรับแต่ง CSS ได้ตามต้องการ */
</style>
