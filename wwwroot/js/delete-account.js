document.addEventListener("DOMContentLoaded", () => {

    const deleteBtn = document.getElementById("deleteAccountBtn");
    const confirmDeleteBtn = document.getElementById("confirmDeleteBtn");

    if (!deleteBtn || !confirmDeleteBtn) return;

    const confirmModal = bootstrap.Modal.getOrCreateInstance(
        document.getElementById("confirmDeleteModal")
    );

    const successModal = bootstrap.Modal.getOrCreateInstance(
        document.getElementById("successModal")
    );

    const errorModal = bootstrap.Modal.getOrCreateInstance(
        document.getElementById("errorModal")
    );

    deleteBtn.addEventListener("click", () => {
        confirmModal.show();
    });

    confirmDeleteBtn.addEventListener("click", async () => {

        confirmDeleteBtn.disabled = true;

        let response;
        let result = {};

        try {
            response = await fetch("/JobSeeker/DeleteAccount", {
                method: "POST"
            });

            try {
                result = await response.json();
            } catch {
                result = { success: false, message: "Unexpected server response." };
            }

        } catch {
            result = { success: false, message: "Network error." };
        }

        confirmDeleteBtn.disabled = false;

        if (response && response.ok) {

            confirmModal.hide();

            document.getElementById("successModalBody").innerText = result.message;
            successModal.show();

            setTimeout(() => {
                window.location.href = "/";
            }, 2000);

        } else {

            document.getElementById("errorModalBody").innerText =
                result.message || "Failed to delete account.";

            errorModal.show();
        }
    });
});
