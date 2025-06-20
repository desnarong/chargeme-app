<script setup>
    defineEmits(['updateSessionToken', 'updateUser', 'changeLanguage']);
</script>
<template>
    <div class="flex flex-col items-center">
        <!-- Top bar with Back icon and Title -->
        <!--<div class="w-full max-w-md flex items-center">-->
            <!-- Back Icon -->
            <!--<button @click="goBack" class="text-gray-500">
                <i class="fas fa-chevron-left text-xl"></i>
            </button>-->
            <!-- Title -->
            <!--<h1 class="text-lg font-semibold ml-2" @click="goBack">{{ translations.CHARGE_STATUS_TITLE }}</h1>
        </div>-->
        <!-- User Picture -->
        <div class="battery">
            <div class="battery-head"></div>
            <div class="battery-head2"></div>
            <div class="battery-body">
                <div class="battery-level" v-for="n in 6" :key="n" :class="{'filled': n <= startLevel}"></div>
                <div v-if="startLevel < fullLevel" class="lightning-icon">
                    <img v-if="currentLevel > 0" src="../assets/lightning.png" alt="Logo" class="w-[30%] ml-[35%]" style="transform: rotate(270deg);"> <!-- ปรับขนาดโลโก้ได้ตามต้องการ -->
                </div>
            </div>
        </div>
        <h1 class="text-xl text-blue-400 font-semibold mt-[-40px]">{{ chargestatus }}</h1>
        <!-- User Name -->

        <h1 class="text-[18px] text-blue-700 font-semibold mt-[80px]">{{ chargedata.charger }} {{ chargedata.header }}</h1>
    </div>
    <div class="grid grid-cols-2 items-center m-5">
        <div class="col-span-2 m-2">
            <p class="text-[16px] text-blue-400 m-2">{{ translations.CHARGE_STATUS_CHARGED_TEXT }}</p>
            <p class="text-[16px] text-blue-400 m-2">{{ translations.CHARGE_STATUS_PERIOD_TEXT }}</p>
            <p class="text-[16px] text-blue-400 m-2">{{ translations.CHARGE_STATUS_MONETARY_TEXT }}</p>
        </div>
        <div class="col-span-2 m-2 text-right">
            <p class="text-[16px] text-blue-400 font-semibold m-2">{{ meter }} kWh</p>
            <p class="text-[16px] text-blue-400 font-semibold m-2">{{ chargetime }}</p>
            <p class="text-[16px] text-blue-400 font-semibold m-2">{{ cost }} {{ translations.CHARGE_STATUS_BAHT_TEXT }}</p>
        </div>

        <div class="col-span-2 m-2">
            <p class="text-[16px] text-blue-400 m-2">{{ translations.CHARGE_STATUS_ITEMNO_TEXT }}</p>
            <p class="text-[16px] text-blue-400 m-2">{{ translations.CHARGE_STATUS_AMOUNT_TEXT }}</p>
        </div>
        <div class="col-span-2 m-2 text-right">
            <p class="text-[16px] text-blue-400 font-semibold m-2">{{ paydata.data.orderNo }}</p>
            <p class="text-[16px] text-blue-400 font-semibold m-2">{{ paydata.data.total }} {{ translations.CHARGE_STATUS_BAHT_TEXT }}</p>
        </div>
        <div class="col-span-4 m-3 text-center">
            <button @click="getqrcode"
                    class="w-[35%] p-5 bg-blue-500 text-white text-[16px] py-2 px-4 rounded hover:bg-blue-700 hover:text-white duration-300">
                {{ translations.CHARGESTOP_BUTTON_TEXT }}
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
        data() {
            return {
                selectedFile: null, // เก็บไฟล์รูปที่ผู้ใช้เลือก
                startLevel: 0,
                fullLevel: 6, // ระดับที่ต้องการแสดง (0-5) แทนระดับ 0% - 100%
                currentLevel: 0, // ระดับปัจจุบัน (เริ่มจาก 0)
                interval: null, // ใช้เก็บ reference ของ interval
                meter: 0,
                cost: 0,
                chargetime: "",
                chargestatus: null,
            };
        },
        beforeDestroy() {
            // หยุด interval เมื่อ component ถูกทำลาย
            clearInterval(this.interval);
        },
        methods: {
            async goBack() {
                await this.$router.push({ name: 'home' });
            },
            async animateBatteryLevel() {
                if (this.interval) clearInterval(this.interval); // เคลียร์ interval เก่าก่อนเริ่มใหม่

                this.interval = setInterval(() => {
                    if (this.startLevel < this.currentLevel) {
                        this.startLevel++;
                    } else if (this.startLevel === this.fullLevel) {
                        clearInterval(this.interval);
                    } else {
                        this.startLevel = 0;
                    }
                }, 1000);
            },
            async setCurrentLevel(level) {
                this.currentLevel = level; // ตั้งค่า currentLevel เป็น fullLevel (5)
            },
            async checkTransactionStatus() {
                try {
                    while (this.currentLevel < this.fullLevel) {
                        const response = await axios.post(`/transaction/status`, {
                            fid: this.transdata.fId,
                            code: this.chargedata.headerID,
                        }, {
                            headers: { Authorization: `Bearer ${this.token}` },
                        });

                        const { data } = response.data;
                        this.chargestatusdata = data;
                        this.transdata = response.data.transdata;

                        this.chargestatus = this.mapChargeStatus(data.fCurrentStatus);

                        this.cost = parseInt(this.transdata.fCost);
                        this.meter = this.chargestatusdata.fCurrentMeter;

                        if (data.status === 'Success' || data.fCurrentStatus === 'Finishing') break;

                        const level = (parseFloat(data.fStateOfCharge) * this.fullLevel) / 100;
                        this.currentLevel = Math.floor(level);

                        this.updateChargeTime();

                        await new Promise(resolve => setTimeout(resolve, 10000));
                    }

                    if (this.currentLevel === this.fullLevel) {
                        setTimeout(() => this.$router.push({ name: 'finish' }), 2000);
                    }
                } catch (error) {
                    console.error('Error checking transaction status:', error);
                }
            },
            mapChargeStatus(status) {
                switch (status) {
                    case 'Available': return 'กรุณาเสียบหัวชาร์จ';
                    case 'Preparing': return 'เตรียมชาร์จ';
                    case 'Charging': return this.translations.CHARGE_STATUS_CHARGING_TEXT;
                    case 'Finishing': return 'จบการทำงาน';
                    default: return 'ไม่ทราบสถานะ';
                }
            },
            updateChargeTime() {
                if (this.transdata.fStartTime && this.chargestatusdata.fCurrentMeterTime) {
                    const start = new Date(this.transdata.fStartTime + 'Z');
                    const current = new Date(this.chargestatusdata.fCurrentMeterTime + 'Z');

                    if (isNaN(start) || isNaN(current)) {
                        console.error('Invalid date format');
                        return;
                    }

                    // คำนวณเวลาที่ต่างกันในวินาที
                    const diff = Math.floor((current - start) / 1000);
                    const hours = Math.floor(diff / 3600);
                    const minutes = Math.floor((diff % 3600) / 60);

                    // แสดงผลลัพธ์
                    if (hours > 0) {
                        this.chargetime = `${hours} ${this.translations.CHARGE_STATUS_HOUR_TEXT} ${minutes} ${this.translations.CHARGE_STATUS_MINUTE_TEXT}`;
                    } else {
                        this.chargetime = `${minutes} ${this.translations.CHARGE_STATUS_MINUTE_TEXT}`;
                    }
                }
            }
        },
        async mounted() {
            this.$emit('update-title', this.translations.CHARGE_STATUS_TITLE);
            await this.animateBatteryLevel();
            await this.checkTransactionStatus();
        },
    };
</script>

<style scoped>
    .battery {
        position: relative;
        width: 100px; /* ความกว้างของแบตเตอรี่ */
        height: 250px; /* ความสูงของแบตเตอรี่ */
        border: 10px solid gray; /* สีขอบ */
        border-radius: 13px; /* มุมกลม */
        background-color: white; /* สีพื้นหลัง */
        transform: rotate(270deg); /* หมุนแบตเตอรี่ไปทางซ้าย */
    }

    .battery-head {
        position: absolute;
        top: -18px; /* ระยะห่างจากแบตเตอรี่ */
        left: 40%; /* กำหนดให้เป็นศูนย์กลาง */
        width: 20px; /* ความกว้างของหัวแบตเตอรี่ */
        height: 10px; /* ความสูงของหัวแบตเตอรี่ */
        background-color: gray; /* สีหัวแบตเตอรี่ */
        border-radius: 2px; /* มุมกลม */
    }

    .battery-head2 {
        position: absolute;
        top: -14px; /* ระยะห่างจากแบตเตอรี่ */
        left: 34%; /* กำหนดให้เป็นศูนย์กลาง */
        width: 30px; /* ความกว้างของหัวแบตเตอรี่ */
        height: 8px; /* ความสูงของหัวแบตเตอรี่ */
        background-color: gray; /* สีหัวแบตเตอรี่ */
        border-radius: 2px; /* มุมกลม */
    }

    .battery-body {
        width: 100%; /* ให้เต็มความกว้าง */
        height: 100%; /* ให้เต็มความสูง */
        border-radius: 20px; /* มุมกลม */
        position: relative;
        display: flex; /* ใช้ flexbox เพื่อให้ชิ้นส่วนเรียงกัน */
        flex-direction: column-reverse; /* ตั้งค่าให้เรียงจากซ้ายไปขวา */
        justify-content: space-between; /* ให้มีระยะห่างระหว่างชิ้น */
    }

    .battery-level {
        flex: 1; /* ให้แต่ละชิ้นมีความกว้างเท่าๆ กัน */
        height: 18%; /* ให้เต็มความสูง */
        margin: 0.25rem 0.15rem 0.25rem 0.15rem; /* เพิ่มระยะห่างด้านซ้ายและขวา */
        border-radius: 5px; /* มุมกลม */
    }
        .battery-level:last-child {
            border-right: none; /* ไม่มีเส้นขอบด้านขวาของชิ้นสุดท้าย */
        }

        .battery-level.filled {
            background-color: #6cc24a; /* สีระดับแบตเตอรี่ที่เต็ม */
        }

    .lightning-icon {
        position: absolute; /* ทำให้ไอคอนอยู่เหนือแบตเตอรี่ */
        top: 10%; /* อยู่กึ่งกลางแนวตั้ง */
        transform: translate(0, -50%); /* ปรับตำแหน่งให้ตรงกลางในแนวตั้ง */
    }
</style>
