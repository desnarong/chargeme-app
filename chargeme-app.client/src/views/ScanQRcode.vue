<template>
    <div class="grid grid-rows-[auto,1fr,auto] place-items-center">
        <!-- Top bar with Back icon and Title -->
        <!--<div class="w-full max-w-md flex items-center">-->
            <!-- Back Icon -->
            <!--<button @click="goBack" class="text-gray-500">
                <i class="fas fa-chevron-left text-xl"></i>
            </button>-->
            <!-- Title -->
            <!--<h1 class="text-lg font-semibold ml-2" @click="goBack">{{ translations.SCAN_QRCODE_TITLE }}</h1>
        </div>-->

        <!-- Flash Icon (แถวใหม่) -->
        <div class="w-full max-w-md px-4 flex justify-end mt-10 h-[10vh]">
            <button @click="toggleFlash" class="text-gray-500">
                <i class="far fa-lightbulb text-xl"></i>
            </button>
        </div>

        <!-- QR Code Scanner Area -->
        <div class="relative w-9/10 max-w-md h-[50vh] flex flex-col items-center justify-center">
            <div class="qr-scanner-border relative w-full h-8/10 border-4 border-green-500 rounded-lg flex items-center justify-center">
                <!-- Camera Preview (สำหรับ Scan QRCode) -->
                <video id="qr-video" class="w-full h-full object-cover"></video>

                <!-- Moving Line (แอนิเมชัน) -->
                <div class="absolute top-0 left-0 w-full h-1 bg-green-500 animate-scan-line"></div>
            </div>

            <!-- Text: ให้ตำแหน่ง QR อยู่ตรงกลางภาพ -->
            <p class="mt-2 text-sm text-gray-500">{{ translations.SCAN_VIDEO_TEXT }}</p>
        </div>

        <!-- Center Icon for Image Upload -->
        <div class="flex items-center justify-center w-full py-4 h-[10vh]">
            <button @click="openImageGallery" class="text-gray-500">
                <i class="fas fa-image text-5xl"></i>
            </button>
            <!-- Hidden File Input -->
            <input ref="fileInput" type="file" @change="handleFileUpload" accept="image/*" class="hidden">
        </div>
    </div>
</template>

<script lang="js">
    import axios from '@/services/api.js';
    import { BrowserQRCodeReader, BrowserQRCodeSvgWriter } from '@zxing/library';

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
                    this.$store.dispatch('savechargedata', value);
                }
            },
        },
        data() {
            return {
                flashEnabled: false,
                qrCodeReader: null,  // สำหรับตัวอ่าน QR Code
            };
        },
        methods: {
            async goBack() {
                if (this.qrCodeReader) {
                    this.qrCodeReader.reset();
                }
                await this.$router.push({ name: 'home' });
            },
            async toggleFlash() {
                this.flashEnabled = !this.flashEnabled;
                // เรียกใช้ API เปิด/ปิด Flash (ขึ้นอยู่กับ device support)
            },
            async openImageGallery() {
                // เปิดหน้าต่างเลือกไฟล์จากแกลเลอรี
                await this.$refs.fileInput.click();
            },
            async startQRScanner() {
                this.qrCodeReader = new BrowserQRCodeReader();
                await this.qrCodeReader.decodeFromVideoDevice(null, 'qr-video', (result, error) => {
                    if (result) {
                        // เมื่อสแกน QR Code สำเร็จ
                        //console.log(result.getText());
                        this.processQRCode(result.getText());
                    }
                });
            },
            async stopQRCodeScanner() {
                if (this.qrCodeReader) {
                    this.qrCodeReader.reset(); // Stops scanning and releases camera
                    this.qrCodeReader = null;
                }
            },
            getIdFromUrl(url) {
                // ตรวจสอบว่า URL มี http:// หรือ https:// ตามด้วย / และค่าหลังจากนั้น
                const regex = /https?:\/\/[^/]+\/(.+)/;
                const match = url.match(regex);

                // ถ้า URL เป็นแบบ https://xxxxx.com/aa78z หรือ http://xxxxx.com/aa78z
                if (match) {
                    return match[1]; // ดึงค่า aa78z ออกมา
                }

                // ถ้า URL เป็นแค่ aa78z
                return url; // คืนค่า url แบบเดิม
            },
            async processQRCode(text) {
                try {

                    const code = this.getIdFromUrl(text);
                    await this.$router.push({ name: 'station', params: { id: code } });

                    ////await this.$router.push({ params: { id: charge } })
                    //// เรียก API โดยใส่ token ใน header
                    //const response = await axios.post(`/api/charger/check`,
                    //    {
                    //        chargeid: code
                    //    },
                    //    {
                    //        headers: {
                    //            'Authorization': `Bearer ${this.token}` // ส่ง token ไปใน header
                    //        }
                    //    });
                    //// ตรวจสอบว่าการเรียก API สำเร็จ
                    //if (response.status === 200) {
                    //    if (response.data.data) {
                    //        const data = response.data;
                    //        this.chargedata = data.data;
                    //        this.paydata = data.pay;
                    //        this.transdata = data.trans;
                    //        //console.log(data.data);
                    //        if (this.paydata.data) {
                    //            if (data.pay.data.status == 'Pending') {
                    //                await this.$router.push({ name: 'qrcode' });
                    //            }
                    //            else if (data.pay.data.status == 'Paid' && data.trans && data.trans.fTransactionStatus == 'Pending') {
                    //                await this.$router.push({ name: 'status' });
                    //            }
                    //            else if (data.pay.data.status == 'Paid' && data.trans && data.trans.fTransactionStatus != 'Charging') {
                    //                await this.$router.push({ name: 'status' });
                    //            }
                    //            else {
                    //                // ใช้ Vue Router เพื่อไปหน้า Station.vue
                    //                await this.$router.push({ params: { id: code } })
                    //            }
                    //        } else {
                    //            await this.$router.push({ params: { id: code } })
                    //        }
                    //    }
                    //    else {
                    //        this.showNoDataAlert();
                    //    }
                    //} else {
                    //    console.error('Failed to create guest session:', response);
                    //}
                } catch (error) {
                    console.error('Error creating guest session:', error);
                }
            },
            async handleFileUpload(event) {
                const file = event.target.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = async (e) => {
                        const imageData = e.target.result;
                        this.qrCodeReader = new BrowserQRCodeReader();
                        try {
                            // ใช้ไลบรารีในการอ่าน QR Code จากรูปภาพ
                            const result = await this.qrCodeReader.decodeFromImage(undefined, imageData);
                            await this.processQRCode(result.getText());
                        } catch (error) {
                            alert("ไม่สามารถอ่าน QR Code จากรูปภาพได้");
                            console.error(error);
                        }
                    };
                    reader.readAsDataURL(file);
                }
            },
        },
        async mounted() {
            this.$emit('update-title', this.translations.SCAN_QRCODE_TITLE);

            // ปิด LoadingSpinner เมื่อหน้า Home ถูกโหลด
            await this.$emit('stopLoading'); // ส่งสัญญาณไปยัง App.vue เพื่อปิด Loading

            //await this.startCamera();
            await this.startQRScanner();

           
        },
        async beforeUnmount() {
            this.stopQRCodeScanner();
        },
        async beforeDestroy() {
            if (this.qrCodeReader) {
                this.qrCodeReader.reset();
            }
        }
    };
</script>

<style scoped>
    .qr-scanner-border {
        position: relative;
        border-radius: 8px;
        overflow: hidden;
    }

    @keyframes scan {
        0% {
            top: 0;
        }

        100% {
            top: 100%;
        }
    }

    .animate-scan-line {
        animation: scan 2s infinite ease-in-out;
    }
</style>