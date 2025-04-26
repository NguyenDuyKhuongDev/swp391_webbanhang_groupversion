/**
 * Avatar Upload Handler
 * Handles image selection, validation, and preview for profile avatars
 * @author tonnahe171051
 * @created 2025-04-23
 */
$(document).ready(function () {
    // Avatar upload handler
    $("#avatarUpload").change(function () {
        const file = this.files[0];
        if (file) {
            // Check file size (limit to 1MB)
            if (file.size > 1024 * 1024) {
                alert("File is too large! Maximum size is 1MB.");
                this.value = "";
                return;
            }

            // Check file type
            if (!file.type.match('image.*')) {
                alert("Only image files are allowed!");
                this.value = "";
                return;
            }

            // Read and convert file to base64
            const reader = new FileReader();
            reader.onload = function (e) {
                // Update preview
                $("#avatarPreview").attr('src', e.target.result);
                // Save base64 to hidden input
                $("#avatarBase64").val(e.target.result);
            };
            reader.readAsDataURL(file);
        }
    });

    // Avatar removal handler
    $("#removeAvatar").click(function () {
        // Reset file input
        $("#avatarUpload").val("");
        // Reset preview to default
        $("#avatarPreview").attr('src', '/images/default-avatar.jpg');
        // Clear base64 value
        $("#avatarBase64").val("");
    });
});