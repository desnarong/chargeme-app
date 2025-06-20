import Cookies from 'js-cookie';
import { v4 as uuidv4 } from 'uuid';

export function getGuestIdentifier() {
    let guestIdentifier = Cookies.get('guestIdentifier');
    if (!guestIdentifier) {
        guestIdentifier = uuidv4();
        Cookies.set('guestIdentifier', guestIdentifier, { expires: 7 }); // หมดอายุใน 7 วัน
    }
    return guestIdentifier;
}