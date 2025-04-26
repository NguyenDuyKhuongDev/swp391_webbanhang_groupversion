
function setupPasswordToggles() {
    // Tìm tất cả các nhóm input mật khẩu có class password-input-group
    document.querySelectorAll('.password-input-group').forEach(function (group) {
        const passwordField = group.querySelector('input[type="password"]');
        const toggleButton = group.querySelector('.password-toggle');

        if (passwordField && toggleButton) {
            // Thêm sự kiện click vào biểu tượng toggle
            toggleButton.addEventListener('click', function () {
                // Chuyển đổi kiểu trường nhập liệu
                const type = passwordField.getAttribute('type') === 'password' ? 'text' : 'password';
                passwordField.setAttribute('type', type);

                // Chuyển đổi biểu tượng mắt
                const icon = toggleButton.querySelector('i');
                icon.classList.toggle('bi-eye');
                icon.classList.toggle('bi-eye-slash');
            });
        }
    });
}

// Thiết lập khi tài liệu đã sẵn sàng
document.addEventListener('DOMContentLoaded', function () {
    setupPasswordToggles();
});
