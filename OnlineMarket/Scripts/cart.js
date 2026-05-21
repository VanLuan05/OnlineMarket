// Scripts/cart.js
class CartManager {
    constructor() {
        this.initEvents();
        console.log("Cart Manager initialized");
    }

    initEvents() {
        // Giảm số lượng
        $(document).on('click', '.quantity-decrease', (e) => {
            this.handleDecreaseQuantity(e);
        });

        // Tăng số lượng
        $(document).on('click', '.quantity-increase', (e) => {
            this.handleIncreaseQuantity(e);
        });

        // Cập nhật số lượng khi nhập trực tiếp
        $(document).on('change', '.quantity-input', (e) => {
            this.handleQuantityChange(e);
        });

        // Xóa sản phẩm
        $(document).on('click', '.remove-item', (e) => {
            this.handleRemoveItem(e);
        });

        // Xóa toàn bộ giỏ hàng
        $('#clear-cart-btn').click((e) => {
            this.handleClearCart(e);
        });
    }

    handleDecreaseQuantity(e) {
        console.log("Decrease clicked");
        // Sử dụng e.currentTarget để bắt chính xác thẻ button, 
        // và closest('.qty-selector') theo giao diện mới
        const input = $(e.currentTarget).closest('.qty-selector').find('.quantity-input');
        const currentVal = parseInt(input.val());
        if (currentVal > 1) {
            input.val(currentVal - 1);
            this.updateQuantity(input);
        }
    }

    handleIncreaseQuantity(e) {
        console.log("Increase clicked");
        // Tương tự, cập nhật selector cho nút Tăng
        const input = $(e.currentTarget).closest('.qty-selector').find('.quantity-input');
        const currentVal = parseInt(input.val());
        const maxVal = parseInt(input.attr('max'));

        if (currentVal < maxVal) {
            input.val(currentVal + 1);
            this.updateQuantity(input);
        } else {
            this.showAlert('warning', 'Đã đạt số lượng tối đa trong kho!');
        }
    }

    handleQuantityChange(e) {
        console.log("Quantity changed");
        const input = $(e.target);
        const currentVal = parseInt(input.val());
        const maxVal = parseInt(input.attr('max'));

        if (currentVal < 1) {
            input.val(1);
            this.showAlert('warning', 'Số lượng phải lớn hơn 0!');
            return;
        }

        if (currentVal > maxVal) {
            input.val(maxVal);
            this.showAlert('warning', 'Đã đạt số lượng tối đa trong kho!');
            return;
        }

        this.updateQuantity(input);
    }

    handleRemoveItem(e) {
        console.log("Remove clicked");
        const cartId = $(e.currentTarget).data('cart-id');
        this.removeFromCart(cartId);
    }

    handleClearCart(e) {
        console.log("Clear cart clicked");
        if (confirm('Bạn có chắc chắn muốn xóa toàn bộ giỏ hàng?')) {
            this.clearCart();
        }
    }

    updateQuantity(input) {
        const cartId = input.data('cart-id');
        const quantity = parseInt(input.val());
        console.log(`Updating cart ${cartId} to quantity ${quantity}`);

        $.post('/Cart/UpdateQuantity', {
            cartId: cartId,
            quantity: quantity
        }).done((response) => {
            console.log("Update response:", response);
            if (response.success) {
                this.updateCartSummary(response);
                this.updateCartCount(response.cartCount);

                // Ẩn dòng thông báo Thành công để tránh làm phiền khi KH click liên tục
                // this.showAlert('success', response.message); 

                // Lấy đơn giá từ cột thứ 3 của bảng thay vì dùng class cũ
                const itemRow = $(`.cart-item[data-cart-id="${cartId}"]`);
                const priceText = itemRow.find('td:nth-child(3)').text().trim();
                const price = parseInt(priceText.replace(/[^0-9]/g, ''));
                const thanhTien = price * quantity;

                itemRow.find('.thanh-tien').text(this.formatCurrency(thanhTien));

            } else {
                this.showAlert('error', response.message);
                location.reload();
            }
        }).fail((xhr, status, error) => {
            console.error("Update error:", error);
            this.showAlert('error', 'Có lỗi xảy ra khi cập nhật giỏ hàng!');
            location.reload();
        });
    }

    removeFromCart(cartId) {
        if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này?')) {
            console.log(`Removing cart item ${cartId}`);

            $.post('/Cart/RemoveFromCart', {
                cartId: cartId
            }).done((response) => {
                console.log("Remove response:", response);
                if (response.success) {
                    $(`.cart-item[data-cart-id="${cartId}"]`).fadeOut(300, function () {
                        $(this).remove();
                    });
                    this.updateCartSummary(response);
                    this.updateCartCount(response.cartCount);
                    this.showAlert('success', response.message);

                    if (response.cartCount === 0) {
                        setTimeout(() => location.reload(), 1000);
                    }
                } else {
                    this.showAlert('error', response.message);
                }
            }).fail((xhr, status, error) => {
                console.error("Remove error:", error);
                this.showAlert('error', 'Có lỗi xảy ra khi xóa sản phẩm!');
            });
        }
    }

    clearCart() {
        $.post('/Cart/ClearCart')
            .done((response) => {
                console.log("Clear cart response:", response);
                if (response.success) {
                    $('.cart-item').fadeOut(300, function () {
                        $(this).remove();
                    });
                    this.updateCartSummary(response);
                    this.updateCartCount(response.cartCount);
                    this.showAlert('success', response.message);
                    setTimeout(() => location.reload(), 1000);
                } else {
                    this.showAlert('error', response.message);
                }
            }).fail((xhr, status, error) => {
                console.error("Clear cart error:", error);
                this.showAlert('error', 'Có lỗi xảy ra khi xóa giỏ hàng!');
            });
    }

    updateCartSummary(response) {
        $('#summary-tongtien').text(this.formatCurrency(response.tongTien));
        $('#summary-phivanchuyen').text(this.formatCurrency(response.phiVanChuyen));
        $('#summary-vat').text(this.formatCurrency(response.vat));
        $('#summary-tongthanhtoan').text(this.formatCurrency(response.tongThanhToan));
        $('#cart-item-count').text(response.cartCount);
    }

    updateCartCount(count) {
        $('#cart-count').text(count);
    }

    formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            minimumFractionDigits: 0
        }).format(amount) + 'đ';
    }

    showAlert(type, message) {
        $('.alert-position-fixed').remove();

        // Nâng cấp Toast Alert sang xịn xò giống trang Shop
        const alertClass = type === 'success' ? 'bg-success text-white' :
            type === 'error' ? 'bg-danger text-white' : 'bg-warning text-dark';
        const iconClass = type === 'success' ? 'fa-check-circle' :
            type === 'error' ? 'fa-exclamation-triangle' : 'fa-exclamation-circle';

        const alertHtml = `
            <div class="alert ${alertClass} alert-dismissible fade show alert-position-fixed shadow-lg"
                 style="position: fixed; top: 30px; right: 30px; z-index: 9999; min-width: 320px; border-radius: 10px; border: none;">
                <div class="d-flex align-items-center">
                    <i class="fas ${iconClass} fs-4 me-3"></i>
                    <div class="fw-medium">${message}</div>
                </div>
                <button type="button" class="btn-close ${type !== 'warning' ? 'btn-close-white' : ''}" data-bs-dismiss="alert" style="top: 50%; transform: translateY(-50%);"></button>
            </div>
        `;

        $('body').append(alertHtml);

        setTimeout(() => {
            $('.alert-position-fixed').fadeOut(500, function () { $(this).remove(); });
        }, 3000);
    }
}

// Khởi tạo khi document ready
$(document).ready(function () {
    new CartManager();
});