<template>
    <div class="grid grid-rows-[auto,1fr,auto] place-items-center">
        <!-- Logo -->
        <div class="mb-2 mt-2 w-full">
            <img :src="'data:image/jpg;base64,' + chargedata.image" alt="Logo" class="w-[90%] mx-auto" />
        </div>

        <!-- Station Icon and Info -->
        <div class="w-[90%] grid grid-cols-4 gap-2">
            <!-- Left: Station Icon -->
            <div class="col-span-1 flex items-center justify-center">
                <img :src="'data:image/jpg;base64,' + chargedata.logo" alt="Station Icon" class="w-[80%] h-auto" />
            </div>
            <!-- Right: Station Info -->
            <div class="col-span-3 flex flex-col justify-center">
                <!--<p class="text-[12px] text-blue-500">{{ translations.CHARGESTATION_TITLE_TEXT }} <span class="text-[14px] font-semibold text-blue-800">{{ translations.CHARGESTATION_TITLE_TEXT2 }}</span></p>-->
                <p class="text-[14px]">{{ chargedata.company }}</p>
                <p class="text-[10px]">{{ chargedata.company_addr }}</p>
            </div>
        </div>

        <p class="text-[14px] font-semibold text-blue-800"><span class="text-[14px] font-semibold text-blue-800">{{
            translations.CHARGESTATION_CHARGER }} เครื่องชาร์จ</span> {{ chargedata.charger }} {{ chargedata.header
                }}</p>

        <!-- Pricing and Input Section -->
        <div class="w-[90%] grid grid-cols-4 gap-1 text-[12px]">
            <div class="col-span-4">
                <p class="text-[14px] text-left mb-2"><span class="font-semibold" style="color: #48BFEA">{{
                    translations.CHARGESTATION_CHOOSE_TEXT }}</span></p>
            </div>
            <button v-for="(price, index) in priceshows" :key="index" @click="setAmount(price.price, price.hour)"
                class="w-full border border-blue-300 text-black-500 font-bold py-2 h-[45px] rounded hover:bg-blue-400 hover:text-white transition duration-300">
                {{ price.price }} {{ price.unit }}
            </button>

            <div class="col-span-4 text-center">
                <span class="text-[14px] font-semibold text-blue-300">{{ translations.CHARGESTATION_MONEY_TEXT }}
                </span>
                <input type="text" maxlength="5" v-model="inputValue" @input="validateNumber" @focus="selectAll"
                    @change="updateChargetotal" @keyup="keyUpChargetotal"
                    class="w-[50%] m-2 border border-gray-300 p-2 text-center rounded focus:outline-none focus:ring-2 focus:ring-blue-500 shadow-inset">
                <span class="text-[14px] font-semibold text-blue-300"> {{ chargedata.unit }}</span>
            </div>
            <div class="col-span-4 text-center">
                <span class="text-[14px] font-semibold text-gray-300">{{ translations.CHARGESTATION_MIN_TEXT }} {{
                    chargedata.minimumAmount }} {{ chargedata.unit }}</span>
            </div>
            <div class="col-span-4 text-center mt-2">
                <span class="text-[14px] font-semibold text-blue-300">{{ translations.CHARGESTATION_CHARGE_TEXT }} {{
                    chargetotal }} kWh {{ translations.CHARGESTATION_MONEY_TEXT }} {{ inputValue }} {{ chargedata.unit
                    }}</span>
            </div>

            <div class="col-span-4 text-center mt-2" v-if="userBalanceKwh > 0">
                <div class="flex-1 flex justify-center h-auto max-w-full">
                    <label class="inline-flex items-center cursor-pointer">
                        <label for="lang-toggle" class="mr-3 text-[14px] font-semibold" style="color: #48BFEA">{{
                            "ใช้หน่วยไฟคงเหลือ" }} {{ userBalanceKwh }} kWh</label>
                        <input id="lang-toggle" type="checkbox" value="" class="sr-only peer"
                            @change="updateChargetotal" v-model="useBalance">
                        <div
                            class="relative w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full rtl:peer-checked:after:-translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:start-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600">
                        </div>
                    </label>
                </div>
            </div>

            <div class="col-span-4 m-3 text-center">
                <button @click="getqrcode"
                    class="w-[100%] p-5 bg-blue-500 text-white text-[14px] py-2 px-4 rounded hover:bg-blue-700 hover:text-white duration-300"
                    style="background-color: #085492">
                    {{ translations.CHARGESTATION_BUTTON_TEXT }}
                </button>
            </div>
        </div>
    </div>
</template>

<script lang="js">
import axios from '@/services/api.js';
import Swal from 'sweetalert2';
import Cookies from 'js-cookie';

export default {
    props: {
        id: String
    },
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
            hour: '0',
            priceshows: [],
            chargetotal: 0,
            userBalanceKwh: 0,
            useBalance: false
        };
    },
    methods: {
        goBack() {
            this.$router.push({ name: 'home' });
        },
        async findCharger(charge) {
            try {
                const response = await axios.post(`/charger/check`, { chargeid: charge }, {
                    headers: { 'Authorization': `Bearer ${this.token}` }
                });
                if (response.status === 200 && response.data.data) {
                    const data = response.data;
                    this.chargedata = data.data;
                    this.paydata = data.pay;
                    this.transdata = data.trans;

                    if (this.paydata.data) {
                        if (data.pay.data.status === 'Pending') {
                            await this.$router.push({ name: 'qrcode' });
                        } else if (data.pay.data.status === 'Paid' && data.trans && ['Pending', 'Waiting', 'Charging'].includes(data.trans.fTransactionStatus)) {
                            await this.$router.push({ name: 'status' });
                        }
                    }
                    this.$emit('update-title', this.chargedata.station);
                } else {
                    this.showNoDataAlert();
                }
            } catch (error) {
                console.error('Error fetching charger:', error);
            }
        },
        async setAmount(amount, hour) {
            this.inputValue = String(amount);
            this.hour = String(hour);
            await this.updateChargetotal();
        },
        validateNumber(event) {
            let value = event.target.value.replace(/[^0-9]/g, '');
            if (value.length > 1) value = value.replace(/^0+/, '');
            this.inputValue = value || ''; // อัปเดตค่าให้สามารถแก้ไขได้
        },
        selectAll(event) {
            event.target.select();
        },
        async updateChargetotal() {
            const amount = parseInt(this.inputValue) || 0;

            if (amount > 0) {
                try {
                    const response = await axios.post(`/charger/cal`, {
                        stationid: this.chargedata.stationID,
                        amount: this.inputValue,
                        hour: this.hour
                    }, {
                        headers: { 'Authorization': `Bearer ${this.token}` }
                    });
                    if (response.status === 200) {
                        this.chargetotal = response.data.data;
                    }
                } catch (error) {
                    console.error('Error calculating charge:', error.response?.data || error);
                }
            }

            if (this.useBalance && this.userBalanceKwh > 0) {
                this.chargetotal = parseFloat(this.chargetotal) + parseFloat(this.userBalanceKwh);
            }
        },
        async keyUpChargetotal() {
            const amount = parseInt(this.inputValue) || 0;

            if (amount > 0) {
                try {
                    const response = await axios.post(`/charger/cal`, {
                        stationid: this.chargedata.stationID,
                        amount: this.inputValue,
                        hour: this.hour
                    }, {
                        headers: { 'Authorization': `Bearer ${this.token}` }
                    });
                    if (response.status === 200) {
                        this.chargetotal = response.data.data;
                    }
                } catch (error) {
                    console.error('Error calculating charge:', error.response?.data || error);
                }
            }

            if (this.useBalance && this.userBalanceKwh > 0) {
                this.chargetotal += this.userBalanceKwh;
            }
        },
        async selectPriceList() {
            try {
                const response = await axios.post(`/charger/priceshows`, {
                    stationid: this.chargedata.stationID
                }, {
                    headers: { 'Authorization': `Bearer ${this.token}` }
                });
                if (response.status === 200) {
                    this.priceshows = response.data.data;
                }
            } catch (error) {
                console.error('Error fetching price list:', error);
            }
        },
        async getqrcode() {

            if (this.chargetotal <= 0) {
                Swal.fire({
                    icon: 'error',
                    title: this.translations.OTHER_ERROR_TITLE,
                    text: "Meter must be greater than 0.",
                    confirmButtonText: this.translations.HOME_POPUP_CLOSE,
                    customClass: { popup: 'swal2-custom-height', confirmButton: 'swal2-confirm-red' }
                }).then((result) => {
                    this.updateChargetotal();
                });
            } else if (this.inputValue < this.chargedata.minimumAmount && this.useBalance == false) {
                Swal.fire({
                    icon: 'error',
                    title: this.translations.OTHER_ERROR_TITLE,
                    text: "Amount must be greater than " + this.chargedata.minimumAmount,
                    confirmButtonText: this.translations.HOME_POPUP_CLOSE,
                    customClass: { popup: 'swal2-custom-height', confirmButton: 'swal2-confirm-red' }
                }).then((result) => {
                    this.inputValue = String(this.chargedata.minimumAmount);
                    this.updateChargetotal();
                });
            }
            else {

                try {
                    const response = await axios.post(`/transaction/payment`, {
                        stationid: this.chargedata.stationID,
                        chargerid: this.chargedata.chargerID,
                        headerid: this.chargedata.headerID,
                        amount: this.inputValue,
                        meter: this.chargetotal,
                    }, {
                        headers: { 'Authorization': `Bearer ${this.token}` }
                    });
                    if (response.status === 200) {
                        this.paydata = response.data.data;
                        this.transdata = response.data.transdata;
                        this.$router.push({ name: 'qrcode' });
                    }
                } catch (error) {
                    console.error('Error generating QR code:', error);
                }
            }
        },
        showNoDataAlert() {
            Swal.fire({
                icon: 'error',
                title: this.translations.OTHER_ERROR_TITLE,
                text: this.translations.HOME_ERROR_TEXT,
                confirmButtonText: this.translations.HOME_POPUP_CLOSE,
                customClass: { popup: 'swal2-custom-height', confirmButton: 'swal2-confirm-red' }
            }).then((result) => {
                if (result.isConfirmed) this.$router.push({ name: 'home' });
            });
        }
    },
    async mounted() {

        try {
            const existCookie = Cookies.get('userCookie-' + this.id);
            const response = await axios.post(`/transaction/connector-status`, {
                connectorCode: this.id,
                ignoreCharging : existCookie == "true"
            }, {
                headers: { Authorization: `Bearer ${this.token}` },
            });

            if (response.status === 200)
            {
                this.$store.dispatch('removePaydata');
                await this.findCharger(this.id);
                await this.selectPriceList();
                this.inputValue = String(this.chargedata.minimumAmount);
                await this.updateChargetotal();
                await this.$emit('stopLoading');
                this.userBalanceKwh = this.user.fBalanceKwh;
            }
            else {
                Swal.fire({
                    icon: 'error',      // ไอคอนแสดงสถานะ (error, warning, info, success)
                    title: this.translations.OTHER_ERROR_TITLE,
                    text: response.message, // ข้อความเพิ่มเติม
                    confirmButtonText: this.translations.HOME_POPUP_CLOSE,  // ปุ่มยืนยัน
                    customClass: {
                        popup: 'swal2-custom-height', // เพิ่มคลาสแบบกำหนดเอง
                        confirmButton: 'swal2-confirm-red', // เพิ่มคลาสสำหรับปุ่ม confirm
                    }
                });
            }
        } catch (error) {
            Swal.fire({
                icon: 'error',      // ไอคอนแสดงสถานะ (error, warning, info, success)
                title: this.translations.OTHER_ERROR_TITLE,
                text: error.response.data.message, // ข้อความเพิ่มเติม
                confirmButtonText: this.translations.HOME_POPUP_CLOSE,  // ปุ่มยืนยัน
                customClass: {
                    popup: 'swal2-custom-height', // เพิ่มคลาสแบบกำหนดเอง
                    confirmButton: 'swal2-confirm-red', // เพิ่มคลาสสำหรับปุ่ม confirm
                }
            });
             this.$router.push({ name: 'home' });
        }


    },
};
</script>

<style scoped>
.swal2-custom-height {
    padding: 2rem 1rem !important;
    min-height: 250px;
}

.swal2-confirm-red {
    background-color: #ff6666 !important;
    color: white !important;
    padding: 10px 50px !important;
}

.shadow-inset {
    box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.1);
    /* เงาด้านใน */
}
</style>