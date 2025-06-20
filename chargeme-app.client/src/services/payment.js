import axios from 'axios';

export const processPayment = (total, detail, email, name, reference) => {
    let config = {
        method: 'post',
        maxBodyLength: Infinity,
        url: `${import.meta.env.VITE_APP_PAYMENT_API_URL}?merchantID=${import.meta.env.VITE_APP_PAYMENT_MERCHANT_ID}&productDetail=ChargeMe&customerEmail=${email}&customerName=${name}&total=${total}&referenceNo=${reference}`,
        headers: {
            'Authorization': `Bearer ${import.meta.env.VITE_APP_PAYMENT_AUTH_TOKEN}`
        }
    };

    return axios.request(config)
        .then(response => {
            //console.log(JSON.stringify(response.data));
            return response; // ส่งผลลัพธ์กลับไปให้ component ใช้ต่อ
        })
        .catch(error => {
            console.error('Error:', error);
            throw error; // ส่ง error กลับไปให้ component ใช้จัดการ
        });
};
