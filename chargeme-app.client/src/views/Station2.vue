<template>
    <div class="grid grid-rows-[auto,1fr,auto] place-items-center">
        <!-- Top bar with Back icon and Title -->
        <div class="w-full max-w-md flex items-center">
            <!-- Back Icon -->
            <button @click="goBack" class="text-gray-500">
                <i class="fas fa-chevron-left text-xl"></i>
            </button>
            <!-- Title -->
            <h1 class="text-lg font-semibold ml-2" style="margin-top: -5px;" @click="goBack">{{ chargedata.station }}</h1>
        </div>

        <!-- Logo -->
        <div class="mb-3 mt-3">
            <img :src="'data:image/jpg;base64,' + chargedata.image" alt="Logo" class="w-[100%] mx-auto" />
        </div>

        <div class="grid grid-cols-4 items-center">

            <!-- คอลัมน์ที่ 1: โลโก้ -->
            <div class="col-span-1 ml-3">
                <img src="../assets/logo-Termfi.png" alt="Logo" class="w-[100%]"> <!-- ปรับขนาดโลโก้ได้ตามต้องการ -->
            </div>

            <!-- คอลัมน์ที่ 2: ข้อความ -->
            <div class="col-span-3 ml-3">
                <p class="text-md text-blue-300">{{ translations.CHARGESTATION_TITLE_TEXT }} <span class="text-[18px] font-semibold text-blue-800">{{ translations.CHARGESTATION_TITLE_TEXT2 }}</span></p>
                <p class="text-[14px]">{{ chargedata.station_addr }}</p>
            </div>
        </div>

        <div class="mb-3 mt-3 items-center">
            <p class="text-lg text-center mb-3"><span class="font-semibold text-blue-800">{{ chargedata.charger }} {{ chargedata.header }}</span></p>
        </div>

        <div class="grid grid-cols-4 gap-1 text-[12px]">
            <div class="col-span-4">
                <p class="text-lg text-left mb-3"><span class="font-semibold text-blue-300">{{ translations.CHARGESTATION_CHOOSE_TEXT }}</span></p>
            </div>
            <button v-for="(price, index) in priceshows"
                    :key="index"
                    @click="setAmount(price.price, price.hour)"
                    class="w-full border border-blue-300 text-black-500 font-bold py-2 h-[45px] rounded hover:bg-blue-400 hover:text-white transition duration-300">
                {{ price.price }} {{ price.unit }}
            </button>

            <div class="col-span-4 text-center">
                <span class="text-[18px] font-semibold text-blue-300">{{ translations.CHARGESTATION_MONEY_TEXT }} </span>
                <input type="text"
                       maxlength="5"
                       v-model="inputValue"
                       @input="validateNumber"
                       @focus="selectAll"
                       @blur="checkMinimum"
                       class="w-[30%] m-2 p-2 border border-gray-300 p-2 text-center rounded shadow-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
                <span class="text-[18px] font-semibold text-blue-300"> {{ chargedata.unit }}</span>
            </div>
            <div class="col-span-4 text-center ml-10">
                <span class="text-[14px] font-semibold text-gray-300">{{ translations.CHARGESTATION_MIN_TEXT }} {{ chargedata.minimumAmount }} {{ chargedata.unit }}</span>
            </div>
            <div class="col-span-4 text-center mt-2">
                <span class="text-[18px] font-semibold text-blue-300">{{ translations.CHARGESTATION_CHARGE_TEXT }} {{ chargetotal }} kWh {{ translations.CHARGESTATION_MONEY_TEXT }} {{ inputValue }} {{ chargedata.unit }}</span>
            </div>

            <div class="col-span-4 m-3 text-center">
                <button @click="getqrcode"
                        class="w-[35%] p-5 bg-blue-500 text-white text-[16px] py-2 px-4 rounded hover:bg-blue-700 hover:text-white duration-300">
                    {{ translations.CHARGESTATION_BUTTON_TEXT }}
                </button>
            </div>
        </div>
    </div>
</template>

<script lang="js">
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
                inputValue: '',
                priceshows: [],
                chargetotal: 0,
            };
        },
        methods: {
            goBack() {
                this.$router.push({ name: 'home' });
            },
            async findCharger(charge) {
                try {
                    // เรียก API โดยใส่ token ใน header
                    const response = await axios.post(`/api/charger/check`,
                        {
                            chargeid: charge
                        },
                        {
                            headers: {
                                'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                            }
                        });
                    // ตรวจสอบว่าการเรียก API สำเร็จ
                    if (response.status === 200) {
                        if (response.data.data) {
                            const data = response.data;
                            this.chargedata = data.data;
                            this.paydata = data.pay;
                            this.transdata = data.trans;
                            //console.log(data.data);
                            if (this.paydata.data) {
                                if (data.pay.data.status == 'Pending') {
                                    await this.$router.push({ name: 'qrcode' });
                                }
                                else if (data.pay.data.status == 'Paid' && data.trans && (data.trans.fTransactionStatus == 'Pending' || data.trans.fTransactionStatus == 'Wating')) {
                                    await this.$router.push({ name: 'status' });
                                }
                                else if (data.pay.data.status == 'Paid' && data.trans && data.trans.fTransactionStatus == 'Charging') {
                                    await this.$router.push({ name: 'status' });
                                }
                                else {
                                    // ใช้ Vue Router เพื่อไปหน้า Station.vue
                                    await this.$router.push({ name: 'chargestation' });
                                }
                            } else {
                                await this.$router.push({ name: 'chargestation' });
                            }
                        }
                        else {
                            this.showNoDataAlert();
                        }
                    } else {
                        console.error('Failed to create guest session:', response);
                    }
                } catch (error) {
                    console.error('Error creating guest session:', error);
                }
            },
            // ฟังก์ชันสำหรับตั้งค่า inputValue เป็นตัวเลขที่ถูกคลิก
            async setAmount(amount, hour) {

                if (this.inputValue === amount) return;

                this.inputValue = amount;
                try {
                    // เรียก API โดยใส่ token ใน header
                    const response = await axios.post(`/api/charger/cal`,
                        {
                            stationid: this.chargedata.stationID,
                            amount: amount,
                            hour: hour
                        },
                        {
                            headers: {
                                'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                            }
                        });
                    // ตรวจสอบว่าการเรียก API สำเร็จ
                    if (response.status === 200) {
                        const data = response.data.data;
                        this.chargetotal = data;

                    } else {
                        console.error('Failed to create guest session:', response);
                    }
                } catch (error) {
                    console.error('Error creating guest session:', error);
                }
            },
            validateNumber() {
                // ลบอักขระที่ไม่ใช่ตัวเลข
                let value = this.inputValue.replace(/[^0-9]/g, '');

                // ลบเลข 0 ที่อยู่ด้านหน้า ยกเว้นในกรณีที่มีแค่ 0 ตัวเดียว
                if (value.length > 1) {
                    value = value.replace(/^0+/, '');
                }

                this.inputValue = value;
            },
            // ฟังก์ชันสำหรับเลือกข้อความทั้งหมดใน input
            selectAll(event) {
                event.target.select(); // เลือกข้อความทั้งหมดใน input
            },
            // ฟังก์ชันตรวจสอบค่าเมื่อเลิก focus
            checkMinimum() {
                if (this.inputValue && parseInt(this.inputValue) < this.chargedata.minimumAmount) {
                    this.inputValue = this.chargedata.minimumAmount; // ตั้งค่าเป็น 60 หากน้อยกว่าขั้นต่ำ
                }
            },
            async selectPriceList() {
                try {
                    // เรียก API โดยใส่ token ใน header
                    const response = await axios.post(`/api/charger/priceshows`,
                        {
                            stationid: this.chargedata.stationID
                        },
                        {
                            headers: {
                                'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                            }
                        });
                    // ตรวจสอบว่าการเรียก API สำเร็จ
                    if (response.status === 200) {
                        const data = response.data.data;
                        this.priceshows = data;
                        
                    } else {
                        console.error('Failed to create guest session:', response);
                    }
                } catch (error) {
                    console.error('Error creating guest session:', error);
                }
            },
            async getqrcode() {
                //const totalAmount = this.inputValue; // หรือค่าที่รับจากการคำนวณ
                //this.user = JSON.parse(decodeURIComponent(userCookie));
                //console.log(this.user.fId);
                try {
                    // เรียก API โดยใส่ token ใน header
                    const response = await axios.post(`/api/transaction/payment`,
                        {
                            stationid: this.chargedata.stationID,
                            chargerid: this.chargedata.chargerID,
                            headerid: this.chargedata.headerID,
                            amount: this.inputValue,
                            meter: this.chargetotal,
                        },
                        {
                            headers: {
                                'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                            }
                        });
                    //console.log(response);
                    // ตรวจสอบว่าการเรียก API สำเร็จ
                    if (response.status === 200) {
                        this.paydata = response.data.data;
                        this.transdata = response.data.transdata;
                        //console.log(this.paydata);
                        this.$router.push({ name: 'qrcode' });
                    } else {
                        console.error('Failed to create guest session:', response);
                    }
                } catch (error) {
                    console.error('Error creating guest session:', error);
                }
            },
        },
        async mounted() {
            //console.log(this.chargedata);
            await this.selectPriceList();

            this.$store.dispatch('removePaydata');

            // ปิด LoadingSpinner เมื่อหน้า Home ถูกโหลด
            await this.$emit('stopLoading'); // ส่งสัญญาณไปยัง App.vue เพื่อปิด Loading
        },
    };
</script>

<style scoped>
    .popup-overlay {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0, 0, 0, 0.5); /* หน้าจอสีมืด */
        display: flex;
        justify-content: center;
        align-items: center;
    }

    .popup-content {
        background-color: white;
        padding: 20px;
        border-radius: 10px;
        text-align: center;
    }

        .popup-content img {
            max-width: 100%;
            height: auto;
        }

    .close {
        position: absolute;
        top: 10px;
        right: 20px;
        font-size: 30px;
        cursor: pointer;
    }
</style>