<template>
    <div class="flex flex-col items-center">
        <div ref="qrContainer" class="rounded-lg p-2">
            <div ref="qrContainer"
                class="border border-gray-300 rounded-lg flex flex-col items-center mt-3 mr-5 ml-5 pb-3">
                <!-- Logo -->
                <div>
                    <img src="../assets/thai_qr_payment.png" alt="Logo"
                        class="w-[100%] mx-auto rounded-lg flex flex-col items-center" />
                </div>
                <img :src="paydata.data.image" v-if="!isExpired && !isSuccess" alt="QR Code" class="w-[75%] h-[75%]" />
                <img src="../assets/coss_red.png" v-if="isExpired" alt="QR Code" class="w-[65%] h-[65%]" />
                <img src="../assets/check_green.png" v-if="isSuccess" alt="QR Code" class="w-[65%] h-[65%]" />
                <h1 class="text-[16px] mt-2">Paysolutions</h1>
                <h1 class="text-[14px]">{{ chargedata.company }} : {{ paydata.data.stationName }}</h1>
                <p class="text-[14px] mt-2 text-lg mb-2 text-red-600" v-if="countdown && !isSuccess && showCountdown">{{
                    countdown }}</p>
            </div>

            <div class="flex flex-col items-center mt-3 mr-5 ml-5 pb-3">
                <button @click="downloadQRCode"
                    class="w-[100%] p-5 bg-blue-500 text-white text-[14px] py-2 px-4 rounded hover:bg-blue-700 hover:text-white duration-300"
                    style="background-color: #085492">
                    Download QRCode
                </button>
            </div>
        </div>
        <!--<a v-if="!isExpired" @click="takeScreenshot" class="text-blue-600">
        {{ translations.QRCODE_SAVEIMAGE_TEXT }}
    </a>-->



        <!--<a ref="downloadLink" style="display: none;">Download</a>-->
        <h1 class="text-[14px] text-blue-800 font-semibold mt-3">{{ chargedata.charger }} {{ chargedata.header }}</h1>
        <h1 class="text-[18px] text-blue-800 font-semibold mb-4">{{ translations.QRCODE_TOTAL_TEXT }} {{
            paydata.data.total }} {{ translations.QRCODE_BAHT_TEXT }}</h1>
        <h1 class="text-[18px] text-blue-800 font-semibold mb-4">{{ translations.NUMBER_OF_UNITS }} {{
            transdata.fPreMeter }} kWh</h1>
        <p class="text-[14px] text-blue-400">{{ translations.QRCODE_MESSAGE1_TEXT }}</p>
        <p class="text-[14px] text-blue-400">{{ translations.QRCODE_MESSAGE2_TEXT }}</p>
    </div>
</template>

<script>
import axios from '@/services/api.js';
import { ref, nextTick } from 'vue';
import html2canvas from 'html2canvas';
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
        chargedata() {
            return this.$store.getters.getChargedata;
        },
        paydata() {
            return this.$store.getters.getPaydata;
        },
        transdata() {
            return this.$store.getters.getTransdata;
        }
    },
    data() {
        return {
            interval: null,
            countdown: '',
            isExpired: false, // สถานะเพื่อบอกว่าหมดเวลาแล้วหรือไม่
            isSuccess: false, // สถานะเพื่อบอกว่าสำเร็จหรือไม่
            isWaitingForPayment: true,
            qrcodedata: {},
            showCountdown: true
        };
    },
    methods: {
        async downloadQRCode() {
            // Hide countdown & payment status temporarily for clean capture
            this.isWaitingForPayment = false;
            this.showCountdown = false;
            await nextTick();

            const qrContainer = this.$refs.qrContainer;
            if (!qrContainer) return;

            const canvas = await html2canvas(qrContainer, { useCORS: true, scale: 2 });
            const blob = await new Promise(resolve => canvas.toBlob(resolve, 'image/png'));

            const file = new File([blob], 'qr-code.png', { type: 'image/png' });

            if (navigator.canShare && navigator.canShare({ files: [file] })) {
                try {
                    await navigator.share({
                        title: 'QR Code',
                        text: 'Save your QR Code image.',
                        files: [file]
                    });
                    console.log('Shared successfully');
                } catch (err) {
                    console.error('Share failed:', err);
                }
            } else {
                // Fallback for unsupported browsers (auto download)
                const link = document.createElement('a');
                link.href = URL.createObjectURL(blob);
                link.download = 'qr-code.png';
                link.click();
            }

            // Restore countdown & payment status after capture
            this.isWaitingForPayment = true;
            this.showCountdown = true;
        },
        async takeScreenshot() {

            // ซ่อน countdown ก่อน
            this.isWaitingForPayment = false;
            this.showCountdown = false; // ถ้ามีตัวแปร showCountdown เพิ่มเข้าไป (หรือเปลี่ยน state เดิมที่มี)

            await nextTick(); // รอ DOM อัปเดตก่อน (สำคัญมาก!)

            const qrContainer = this.$refs.qrContainer;
            const downloadLink = this.$refs.downloadLink;

            if (qrContainer) {
                const canvas = await html2canvas(qrContainer);
                const imageData = canvas.toDataURL('image/png');

                const now = new Date();
                const year = now.getFullYear();
                const month = String(now.getMonth() + 1).padStart(2, '0');
                const day = String(now.getDate()).padStart(2, '0');
                const hours = String(now.getHours()).padStart(2, '0');
                const minutes = String(now.getMinutes()).padStart(2, '0');
                const seconds = String(now.getSeconds()).padStart(2, '0');

                const filename = `qrcode_${year}${month}${day}_${hours}${minutes}${seconds}.png`;

                downloadLink.href = imageData;
                downloadLink.download = filename;
                downloadLink.click();
            }

            // แสดง countdown กลับ
            this.isWaitingForPayment = true;
            this.showCountdown = true;
        },
        async updateCountdown() {
            if (this.paydata.data) {
                const expireDateTime = new Date(this.paydata.data.expiredate).getTime() - 10000; // แปลง expireDate เป็น timestamp
                const now = new Date().getTime();
                const timeLeft = expireDateTime - now; // คำนวณเวลาที่เหลือ
                if (timeLeft < 0) {
                    this.countdown = this.translations.QRCODE_EXPIRED_TEXT; // เมื่อหมดเวลา
                    this.isExpired = true; // เปลี่ยนสถานะให้หมดเวลา
                    this.isWaitingForPayment = false;
                    try {
                        // เรียก API โดยใส่ token ใน header
                        const response = await axios.post(`/payment/cancel`,
                            {
                                paymentid: this.paydata.data.fid,
                            },
                            {
                                headers: {
                                    'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                                }
                            });
                        // ตรวจสอบว่าการเรียก API สำเร็จ
                        if (response.status == 200) {
                            clearInterval(this.interval);
                        }
                    } catch (error) {
                        console.error('Error creating guest session:', error);
                    }
                } else {
                    const seconds = Math.floor((timeLeft % (1000 * 60)) / 1000);
                    const minutes = Math.floor((timeLeft % (1000 * 60 * 60)) / (1000 * 60));

                    this.countdown = `${minutes} : ${seconds} ${this.translations.CHARGE_STATUS_MINUTE_TEXT}`; // แสดงเวลาที่เหลือ
                }
            }
        },
        stopCountdown() {
            // หยุด interval โดยใช้ clearInterval
            if (this.interval) {
                clearInterval(this.interval);
                this.interval = null; // ล้างค่า interval ID
            }
        },
        async waitForPayment() {
            // รอการชำระเงินจาก server
            while (this.isWaitingForPayment) {
                // คำขอเพื่อเช็คสถานะการชำระเงิน
                if (this.paydata.data) {
                    const response = await axios.post(`/payment/status`,
                        {
                            paymentid: this.paydata.data.fid,
                        },
                        {
                            headers: {
                                'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                            }
                        });
                    if (response.data.data.status === 'paid') {
                        this.isSuccess = true;
                        clearInterval(this.interval);
                        break; // ออกจาก loop หากได้รับการชำระเงิน
                    }

                    await new Promise((resolve) => setTimeout(resolve, 5000)); // รอ 5 วินาทีก่อนเช็คสถานะใหม่
                }
            }

            if (this.isSuccess) {
                setTimeout(() => {
                    this.$router.push({ name: 'status' });
                }, 5000); // หน่วงเวลา 5 วินาที (5000 มิลลิวินาที)
            }
        },
    },
    async mounted() {
        this.$emit('update-title', this.translations.QRCODE_TITLE);
        this.stopCountdown();
        //this.updateCountdown();
        this.interval = setInterval(this.updateCountdown, 1000); // อัปเดตทุกวินาที
        // เริ่มรอการชำระเงิน
        await this.waitForPayment();
    },
    async beforeUnmount() {
        // หยุด interval ก่อนที่ component จะถูกทำลาย
        this.stopCountdown();
        //clearInterval(this.interval);

        if (!this.isSuccess && !this.isExpired) {
            try {
                // เรียก API โดยใส่ token ใน header
                const response = await axios.post(`/payment/cancel`,
                    {
                        paymentid: this.paydata.data.fid,
                    },
                    {
                        headers: {
                            'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                        }
                    });
                // console.log('cancel', response);
                // ตรวจสอบว่าการเรียก API สำเร็จ
                if (response.status != 200) {
                    console.error('Failed to create guest session:', response);
                }
            } catch (error) {
                console.error('Error creating guest session:', error);
            }
        }
    }
};
</script>
