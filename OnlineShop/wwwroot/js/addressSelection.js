/**
 * Vietnam Address Selection
 * Handles province/district/ward selection via API
 * @author tonnahe171051
 * @updated 2025-04-23 03:59:45
 */
$(document).ready(function () {
    // Get current values from the form
    const initialProvince = $("#currentProvinceName").val() || '';
    const initialDistrict = $("#currentDistrictName").val() || '';
    const initialWard = $("#currentWardName").val() || '';

    // Add custom validation methods
    $.validator.addMethod(
        "validProvince",
        function (value, element) {
            return value != "" && value != "0";
        },
        "Please select a province/city"
    );

    $.validator.addMethod(
        "validDistrict",
        function (value, element) {
            return value != "" && value != "0";
        },
        "Please select a district"
    );

    $.validator.addMethod(
        "validWard",
        function (value, element) {
            return value != "" && value != "0";
        },
        "Please select a ward/commune"
    );

    // Add validation rules if form validation is initialized
    if ($.validator && $("#updateProfileForm").length) {
        try {
            $("#currentProvinceName").rules("add", { validProvince: true });
            $("#currentDistrictName").rules("add", { validDistrict: true });
            $("#currentWardName").rules("add", { validWard: true });
        } catch (e) {
            console.log("Form validation not initialized yet");
        }
    }

    // Form submission validation
    $("#updateProfileForm").submit(function (e) {
        // Check if user is entering address information
        var hasEnteredAnyAddressField =
            ($("#tinh1").val() != "0") ||
            ($("#quan1").val() != "0") ||
            ($("#phuong1").val() != "0") ||
            ($("#AddressDetail").val().trim() !== "");

        // If entering address, all fields must be filled
        if (hasEnteredAnyAddressField) {
            var isProvinceSelected = $("#tinh1").val() != "0";
            var isDistrictSelected = $("#quan1").val() != "0";
            var isWardSelected = $("#phuong1").val() != "0";
            var hasAddressDetail = $("#AddressDetail").val().trim() !== "";

            var addressIsValid = isProvinceSelected && isDistrictSelected && isWardSelected && hasAddressDetail;

            // Clear current validation messages
            $("[data-valmsg-for]").text("");

            // Show error messages if needed
            if (!isProvinceSelected) {
                $("[data-valmsg-for='Province']").text("Tỉnh/Thành phố là bắt buộc");
            }

            if (!isDistrictSelected) {
                $("[data-valmsg-for='District']").text("Quận/Huyện là bắt buộc");
            }

            if (!isWardSelected) {
                $("[data-valmsg-for='Ward']").text("Phường/Xã là bắt buộc");
            }

            if (!hasAddressDetail) {
                $("[data-valmsg-for='AddressDetail']").text("Địa chỉ chỉ tiết là bắt buộc");
            }

            // Update hidden fields with correct values
            if (isProvinceSelected) {
                $("#currentProvinceName").val($("#tinh1 option:selected").text());
            } else {
                $("#currentProvinceName").val("");
            }

            if (isDistrictSelected) {
                $("#currentDistrictName").val($("#quan1 option:selected").text());
            } else {
                $("#currentDistrictName").val("");
            }

            if (isWardSelected) {
                $("#currentWardName").val($("#phuong1 option:selected").text());
            } else {
                $("#currentWardName").val("");
            }

            // If address is invalid, prevent form submission
            if (!addressIsValid) {
                e.preventDefault();
                // Scroll to first error message
                if ($(".text-danger:visible").length) {
                    $('html, body').animate({
                        scrollTop: $(".text-danger:visible:first").offset().top - 100
                    }, 200);
                }
                return false;
            }
        } else {
            // If not entering any address, set all address fields to empty
            $("#currentProvinceName").val("");
            $("#currentDistrictName").val("");
            $("#currentWardName").val("");
            $("#AddressDetail").val("");
        }

        // Allow form submission if everything is valid
        return true;
    });

    // Khai báo handlers trước khi load dữ liệu
    let provinceChangeHandler = function (e) {
        var idtinh = $(this).val();
        var tinhName = $("#tinh1 option:selected").text();
        $("#currentProvinceName").val(tinhName === "Province/City" ? "" : tinhName);

        if (tinhName !== "Province/City") {
            $("[data-valmsg-for='Province']").text("");
        }

        // Load districts
        $.getJSON('https://esgoo.net/api-tinhthanh/2/' + idtinh + '.htm', function (data_quan) {
            if (data_quan.error == 0) {
                $("#quan1").html('<option value="0">District</option>');
                $("#phuong1").html('<option value="0">Ward/Commune</option>');

                $.each(data_quan.data, function (key_quan, val_quan) {
                    var selected = val_quan.full_name === initialDistrict ? ' selected' : '';
                    $("#quan1").append('<option value="' + val_quan.id + '"' + selected + '>' + val_quan.full_name + '</option>');
                });

                if (initialDistrict) {
                    $("#currentDistrictName").val(initialDistrict);
                }

                // Nếu có giá trị ban đầu và quan1 không phải giá trị mặc định
                if ($("#quan1").val() != "0") {
                    $("#quan1").trigger('change');
                }
            }
        });
    };

    let districtChangeHandler = function (e) {
        var idquan = $(this).val();
        var quanName = $("#quan1 option:selected").text();
        $("#currentDistrictName").val(quanName === "District" ? "" : quanName);

        if (quanName !== "District") {
            $("[data-valmsg-for='District']").text("");
        }

        // Load wards
        $.getJSON('https://esgoo.net/api-tinhthanh/3/' + idquan + '.htm', function (data_phuong) {
            if (data_phuong.error == 0) {
                $("#phuong1").html('<option value="0">Ward/Commune</option>');

                $.each(data_phuong.data, function (key_phuong, val_phuong) {
                    var selected = val_phuong.full_name === initialWard ? ' selected' : '';
                    $("#phuong1").append('<option value="' + val_phuong.id + '"' + selected + '>' + val_phuong.full_name + '</option>');
                });

                if (initialWard) {
                    $("#currentWardName").val(initialWard);
                }
            }
        });
    };

    let wardChangeHandler = function (e) {
        var phuongName = $("#phuong1 option:selected").text();
        $("#currentWardName").val(phuongName === "Ward/Commune" ? "" : phuongName);

        if (phuongName !== "Ward/Commune") {
            $("[data-valmsg-for='Ward']").text("");
        }
    };

    // Đăng ký event handlers
    $("#tinh1").change(provinceChangeHandler);
    $("#quan1").change(districtChangeHandler);
    $("#phuong1").change(wardChangeHandler);

    // Load provinces/cities
    $.getJSON('https://esgoo.net/api-tinhthanh/1/0.htm', function (data_tinh) {
        if (data_tinh.error == 0) {
            $.each(data_tinh.data, function (key_tinh, val_tinh) {
                var selected = val_tinh.full_name === initialProvince ? ' selected' : '';
                $("#tinh1").append('<option value="' + val_tinh.id + '"' + selected + '>' + val_tinh.full_name + '</option>');
            });

            if (initialProvince) {
                $("#currentProvinceName").val(initialProvince);
            }

            // Chạy trigger CUỐI CÙNG sau khi tất cả handlers đã được đăng ký
            if ($("#tinh1").val() != "0") {
                $("#tinh1").trigger('change');
            }
        }
    });
});