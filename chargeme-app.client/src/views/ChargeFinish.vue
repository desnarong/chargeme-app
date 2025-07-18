<template>
    <div class="flex flex-col items-center">
        <!-- Top bar with Back icon and Title -->
        <!--<div class="w-full max-w-md flex items-center">-->
            <!-- Back Icon -->
            <!--<button @click="goBack" class="text-gray-500">
                <i class="fas fa-chevron-left text-xl"></i>
            </button>-->
            <!-- Title -->
            <!--<h1 class="text-lg font-semibold ml-2" @click="goBack">{{ translations.CHARGE_FINISH_TITLE }}</h1>
        </div>-->
        <h1 class="text-[22px] font-semibold text-blue-700">{{ translations.CHARGE_FINISH_TITLE }}</h1>
        <!-- Picture -->
        <div class="relative w-1/5 mt-5">
            <img src="../assets/logo-Termfi.png" alt="User Picture" class="w-full h-auto object-cover" />
        </div>
        <!-- ข้อความ -->
        <h1 class="text-[18px] font-semibold mt-5 text-blue-700">{{ translations.CHARGE_FINISH_HEADER_TEXT }}</h1>
        <p class="text-[13px] font-semibold mt-3 text-gray-600 mr-[50px] ml-[50px] text-center">{{ chargedata.station_addr }}</p>
        <p class="text-[13px] font-semibold text-gray-600 mr-10 ml-10 text-center">{{ translations.CHARGE_FINISH_TAX_TEXT }} {{ chargedata.tax }}</p>
        <h1 class="text-[18px] font-semibold mt-3 text-blue-700">{{ translations.CHARGE_FINISH_THANKYOU_TEXT }}</h1>
    </div>
    <div class="grid grid grid-cols-2 gap-4 items-center mt-3">
        <div class="col-span-1 m-2">
            <p class="text-[18px] text-blue-600 m-2">{{ translations.CHARGE_FINISH_REFERENCE_TEXT }}</p>
            <p class="text-[18px] text-blue-600 m-2">{{ translations.CHARGE_FINISH_CHARGING_STATION_TEXT }}</p>
            <p class="text-[18px] text-blue-600 m-2">{{ translations.CHARGE_FINISH_CHARGER_TEXT }}</p>
            <p class="text-[18px] text-blue-600 m-2">{{ translations.CHARGE_FINISH_START_TEXT }}</p>
            <p class="text-[18px] text-blue-600 m-2">{{ translations.CHARGE_FINISH_END_TEXT }}</p>
            <p class="text-[18px] text-blue-600 m-2">{{ translations.CHARGE_FINISH_TIME_TEXT }}</p>
            <p class="text-[18px] text-blue-600 m-2">{{ translations.CHARGE_FINISH_ENERGYUSED_TEXT }}</p>
        </div>
        <div class="col-span-1 m-2 text-right">
            <p class="text-[18px] text-blue-600 font-semibold m-2">{{ paydata.data.orderNo }}</p>
            <p class="text-[18px] text-blue-600 font-semibold m-2">{{ chargedata.station }}</p>
            <p class="text-[18px] text-blue-600 font-semibold m-2">{{ chargedata.charger }} {{ chargedata.header }}</p>
            <p class="text-[18px] text-blue-600 font-semibold m-2">{{ formatDate(transdata.fStartTime) }}</p>
            <p class="text-[18px] text-blue-600 font-semibold m-2">{{ formatDate(transdata.fEndTime) }}</p>
            <p class="text-[18px] text-blue-600 font-semibold m-2">{{ getTimeDifference(transdata.fStartTime, transdata.fEndTime) }}</p>
            <p class="text-[18px] text-blue-600 font-semibold m-2">{{ transdata.fPreMeter }} kWh</p>
        </div>
        <!-- เส้นประคั่นระหว่างส่วน -->
        <div class="col-span-2 mr-2 ml-2 text-right">
            <hr class="border-t border-blue-600 border-dashed w-full" />
        </div>
        <div class="col-span-1 m-2">
            <p class="text-[18px] text-blue-600 font-semibold m-2">{{ translations.CHARGE_FINISH_TOTAL_TEXT }}</p>
            <p v-show="transdata.fPreMeter > transdata.fMeterEnd" class="text-[18px] text-blue-600 font-semibold m-2">มีหน่วยคงเหลือ</p>
        </div>
        <div class="col-span-1 m-2 text-right">
            <p class="text-[18px] text-blue-600 font-semibold m-2">{{ paydata.data.total }} {{ translations.CHARGE_FINISH_BAHT_TEXT }}</p>
            <p v-show="transdata.fPreMeter > transdata.fMeterEnd" class="text-[18px] text-blue-600 font-semibold m-2">{{ (transdata.fPreMeter - transdata.fMeterEnd).toFixed(2) }} kWh</p>
        </div>
    </div>
    <p class="text-[18px] text-blue-300 font-semibold mt-10 text-center">{{ translations.CHARGE_FINISH_MESSAGE_TEXT }}</p>
    <div class="mt-5 text-center">
         <button @click="goBack"
                class="w-[35%] p-5 bg-blue-500 text-white text-[16px] py-2 px-4 rounded hover:bg-blue-700 hover:text-white duration-300">
                กลับสู่หน้าหลัก
            </button>
    </div>
</template>

<script>
    import dayjs from "dayjs";
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
            paydata() {
                return this.$store.getters.getPaydata;
            },
            chargedata() {
                return this.$store.getters.getChargedata;
            },
            transdata: {
                get() {
                    return this.$store.getters.getTransdata;
                },
                set(value) {
                    this.$store.dispatch('updateTransdata', value);
                }
            },
            chargestatusdata: {
                get() {
                    return this.$store.getters.getChargestatusdata;
                },
                set(value) {
                    this.$store.dispatch('updateChargestatusdata', value);
                }
            },
        },
        methods: {
            async goBack() {
                await this.$router.push({ name: 'home' });
            },
            async checkTransactionStatus() {
                try {
                    const response = await axios.post(`/transaction/status`, {
                        fid: this.transdata.fId,
                        code: this.chargedata.headerID,
                    }, {
                        headers: { Authorization: `Bearer ${this.token}` },
                    });

                    const { data } = response.data;
                    this.chargestatusdata = data;
                    this.transdata = response.data.transdata;
                } catch (error) {
                    console.error('Error checking transaction status:', error);
                }
            },
            formatDate(dateString) {
                const date = new Date(dateString);

                // เพิ่มเวลา +7 ชั่วโมง (7 * 60 * 60 * 1000 milliseconds)
                date.setHours(date.getHours() + 7);

                return new Intl.DateTimeFormat('th-TH', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit',
                }).format(date);
            },
            getTimeDifference(start, end) {
                const startDate = dayjs(start);
                const endDate = dayjs(end);

                // หาความต่างเป็นนาที
                const diffMinutes = endDate.diff(startDate, "minute");

                if (diffMinutes < 60) {
                    return `${diffMinutes} ${this.translations.CHARGE_FINISH_MINUTE_TEXT}`;
                } else {
                    const hours = Math.floor(diffMinutes / 60); // ชั่วโมง
                    const minutes = diffMinutes % 60; // นาทีที่เหลือ
                    return `${hours || 0} ชั่วโมง ${minutes || 0} ${this.translations.CHARGE_FINISH_MINUTE_TEXT}`;
                }
            },
        },
        async mounted() {
            this.$emit('update-title', this.translations.CHARGE_FINISH_TITLE);
            await this.checkTransactionStatus();
        },
    };
</script>

<style scoped>
    /* ปรับแต่ง CSS ได้ตามต้องการ */
</style>
