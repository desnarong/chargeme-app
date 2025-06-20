var SwalFunc = (function () {
    return {
        Close: function () {
            Swal.close();
        },
        Message: function (message) {
            Swal.fire(message);
        },
        Loading: function () {
            Swal.fire({
                title: "ระบบกำลังทำงาน",
                text: "กรุณารอสักครู่...",
                // imageUrl: '/frontend/image/icons/nav_logo.gif',
                imageUrl: "/frontend/image/logo.png",
                showConfirmButton: false,
                showCancelButton: false,
                allowOutsideClick: false,
                allowEscapeKey: false,
                imageHeight: 150,
                onOpen: function () {
                    swal.showLoading()
                }
                //didOpen: () => {
                //    Swal.showLoading();
                //},
            });
        },
        Loading2C2P: function (message) {
            Swal.fire({
                title: `เรากำลังนำท่านไปยังหน้าชำระเงิน หากท่านจำระเงินเสร็จแล้วกรุณากดปุ่ม "กลับมายังร้านค้า" เพื่อพิมพ์ใบเสร็จ`,
                text: "กรุณารอสักครู่...",
                // imageUrl: '/frontend/image/icons/nav_logo.gif',
                imageUrl: "/frontend/image/logo.png",
                showConfirmButton: false,
                showCancelButton: false,
                allowOutsideClick: false,
                allowEscapeKey: false,
                imageHeight: 150,
                // didOpen: () => {
                //     Swal.showLoading();
                // },
            });
        },
        Success: function (header, message) {
            Swal.fire(header, message, "success");
        },
        SuccessWithRedirect: function (header, message, href) {
            Swal.fire({
                type: "success",
                title: header,
                text: message,
                showConfirmButton: false,
                allowOutsideClick: false,
                allowEscapeKey: false,
            });
            setTimeout(function () {
                window.location.href = href;
            }, 1500);
        },
        SuccessWithReload: function (header, message) {
            Swal.fire({
                type: "success",
                title: header,
                text: message,
                showConfirmButton: false,
                allowOutsideClick: false,
                allowEscapeKey: false,
            });
            setTimeout(function () {
                location.reload();
            }, 1500);
        },
        Error: function (header, message) {
            Swal.fire({
                type: "error",
                title: header,
                text: message,
                confirmButtonText: "ตกลง",
                allowOutsideClick: false,
                allowEscapeKey: false,
            });
        },
        ErrorWithReload: function (header, message) {
            Swal.fire({
                type: "error",
                title: header,
                text: message,
                showConfirmButton: false,
            });
            setTimeout(function () {
                location.reload();
            }, 1500);
        },
        ConfirmAjaxDelete: function (id, url, method, element) {
            swal.fire({
                type: 'warning',
                title: "คุณต้องการจะลบใช่หรือไม่",
                showCancelButton: true,
                confirmButtonText: `ลบ`,
                cancelButtonText: `ยกเลิก`,
                allowOutsideClick: true,
                allowEscapeKey: true,
                customClass: {
                    confirmButton: "btn btn-danger mr-2",
                    cancelButton: "btn btn-default",
                },
                buttonsStyling: false,
            }).then((result) => {
                console.log(result);
                /* Read more about isConfirmed, isDenied below */
                if (result.value == true) {
                    if (typeof AjaxConfirmProcess === "function") {
                        AjaxConfirmProcess(id, url, method, element);
                    } else {
                        DeleteItemFunc.ConfirmProcess(id, url, method, element);
                    }
                }
                else {
                    console.log("failed");
                }
            });
        },
        ConfirmAjaxDeleteWithReload: function (id, url, method, element) {
            swal.fire({
                type: 'warning',
                title: "Do you want to delete it?",
                showCancelButton: true,
                confirmButtonText: `ลบ`,
                cancelButtonText: `ยกเลิก`,
                allowOutsideClick: true,
                allowEscapeKey: true,
                customClass: {
                    confirmButton: "btn btn-danger mr-2",
                    cancelButton: "btn btn-default",
                },
                buttonsStyling: false,
            }).then((result) => {
               
                /* Read more about isConfirmed, isDenied below */
                if (result.value == true) {
                    if (typeof AjaxConfirmProcessWithReload === "function") {
                        AjaxConfirmProcessWithReload(id, url, method, element);
                    } else {
                        DeleteItemFunc.ConfirmProcess(id, url, method, element);
                    }
                }
                else {
                    console.log("failed");
                }
            });
        },
        ConfirmUrlDelete: function (url) {
            Swal.fire({
                type: "warning",
                title: "คุณต้องการจะลบใช่หรือไม่",
                showCancelButton: true,
                confirmButtonText: `ลบ`,
                cancelButtonText: `ยกเลก`,
                allowOutsideClick: false,
                allowEscapeKey: false,
                customClass: {
                    confirmButton: "btn btn-danger mr-2",
                    cancelButton: "btn btn-default",
                },
                buttonsStyling: false,
            }).then((result) => {
                /* Read more about isConfirmed, isDenied below */
                if (result.isConfirmed) {
                    window.location.href = url;
                }
            });
        },
    };
})();

function AjaxConfirmProcess(id, url, method, element) {
    $.ajax({
        url: url,
        type: method,
        data: { id: id },
        success: function (data) {
            if (data == 'success') {
                $(element + id).remove();
            }
        }
    });
}

function AjaxConfirmProcessWithReload(id, url, method, element) {
    $.ajax({
        url: url,
        type: method,
        data: { id: id },
        success: function (data) {
            if (data == 'success') {
                element.ajax.reload();
            } else if (data == 'default') {
                console.log(data);
                SwalFunc.Error("ไม่สามารถลบค่าเริ่มต้นได้");
            }
        }
    });
}
