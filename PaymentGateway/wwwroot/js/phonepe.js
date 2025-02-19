//coupon code
const originalAmount = 1489;
const discountedAmount = 997;
const validCouponCode = 'DSCAGENT997';

const couponCheckbox = document.getElementById('coupon');
const couponFieldContainer = document.getElementById('coupon-field-container');
const couponCodeInput = document.getElementById('coupon-code');
const couponStatus = document.getElementById('coupon-status');
const amountDisplay = document.getElementById('amount-display');

let couponApplied = false;

couponCheckbox.addEventListener('change', function () {
    couponFieldContainer.classList.toggle('visible');
    if (!couponCheckbox.checked) {
        resetCoupon();
    }
});

couponCodeInput.addEventListener('input', function () {
    const code = couponCodeInput.value.trim().toUpperCase();

    if (code === validCouponCode && !couponApplied) {
        applyCoupon();
    } else if (code !== validCouponCode && couponApplied) {
        resetCoupon();
    }
});

function applyCoupon() {
    couponApplied = true;
    amountDisplay.innerHTML = `<span class="original-amount">₹ ${originalAmount}.00</span>₹ ${discountedAmount}.00`;
    couponStatus.innerHTML = 'Coupon applied successfully!';
    couponStatus.className = 'coupon-status success-message';
}

function resetCoupon() {
    couponApplied = false;
    amountDisplay.textContent = `₹ ${originalAmount}.00`;
    couponStatus.innerHTML = '';
    couponStatus.className = 'coupon-status';
    couponCodeInput.value = '';
}

//phonepe gateway
document.getElementById("pay-button").addEventListener("click", async function () {
    const paymentData = {
        TransactionId: "TXN123456",
        Amount: 10000,
        CallbackUrl: "/api/payment/callback",
        MobileNumber: "9876543210"
    };

    try {
        const response = await fetch("/api/payment/initiate", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(paymentData)
        });
        console.warn(response);
        const result = await response.json();
        console.warn(result);
        
        if (result) {
            window.location.href = result.result; // Redirect to the payment page
        } else {
            alert("Payment initiation failed: " + result.message);
        }
    } catch (error) {
        console.error("Error initiating payment:", error);
        alert("Error processing payment. Please try again.");
    }
});