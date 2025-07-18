import axios from 'axios';

export const processPayment = (total, detail, email, name, reference) => {
    let config = {
        method: 'post',
        maxBodyLength: Infinity,
        url: `${import.meta.env.VITE_APP_PAYMENT_API_URL}?merchantID=${import.meta.env.VITE_APP_PAYMENT_MERCHANT_ID}&productDetail=ChargeMe&customerEmail=${email}&customerName=${name}&total=${total}&refno=${reference}&cardtype=PP&chargetype=PP`,
        headers: {
            'Authorization': `Bearer ${import.meta.env.VITE_APP_PAYMENT_AUTH_TOKEN}`
        }
    };

    return axios.request(config)
        .then(response => {
            //console.log(JSON.stringify(response.data));
            return response; // �觼��Ѿ���Ѻ���� component ����
        })
        .catch(error => {
            console.error('Error:', error);
            throw error; // �� error ��Ѻ���� component ��Ѵ���
        });
};
