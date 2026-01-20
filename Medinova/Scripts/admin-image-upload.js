(function () {
    const containers = document.querySelectorAll('[data-image-upload]');

    const setupContainer = (container) => {
        const fileInput = container.querySelector('[data-image-upload-input]');
        const previewWrapper = container.querySelector('[data-image-upload-preview]');
        const previewImage = container.querySelector('[data-image-upload-preview-image]');
        const uploadArea = container.querySelector('[data-image-upload-area]');
        const removeButton = container.querySelector('[data-image-upload-remove]');
        const fileInfo = container.querySelector('[data-image-upload-info]');
        const urlInput = container.querySelector('[data-image-upload-url]');
        const clearUrlButton = container.querySelector('[data-image-upload-clear-url]');
        const previewAvatar = container.querySelector('[data-image-upload-avatar]');
        const avatarOptions = container.querySelector('[data-image-upload-avatar-options]');
        const defaultAvatarClass = previewAvatar?.dataset.defaultClass;
        const defaultAvatarIcon = previewAvatar?.dataset.defaultIcon;
        const defaultAvatarIconClass = previewAvatar?.dataset.defaultIconClass;
        const initialFileRequired = fileInput ? fileInput.required : false;

        const setPreview = (url, file) => {
            if (previewImage) {
                previewImage.style.backgroundImage = `url('${url}')`;
            }

            if (previewAvatar) {
                previewAvatar.style.backgroundImage = `url('${url}')`;
                previewAvatar.innerHTML = '';
            }

            if (avatarOptions) {
                avatarOptions.classList.add('hidden');
            }

            if (fileInfo) {
                if (file) {
                    fileInfo.textContent = `${file.name} (${formatFileSize(file.size)})`;
                } else if (fileInfo.dataset.defaultText) {
                    fileInfo.textContent = fileInfo.dataset.defaultText;
                }
            }

            if (previewWrapper) {
                previewWrapper.classList.remove('hidden');
            }
            if (uploadArea) {
                uploadArea.classList.add('hidden');
            }
        };

        const resetPreview = () => {
            if (fileInput) {
                fileInput.value = '';
                fileInput.required = initialFileRequired;
            }
            if (previewImage) {
                previewImage.style.backgroundImage = '';
            }
            if (previewAvatar) {
                previewAvatar.style.backgroundImage = '';
                if (defaultAvatarClass) {
                    previewAvatar.className = defaultAvatarClass;
                }
                if (defaultAvatarIcon) {
                    const iconClass = defaultAvatarIconClass ? ` ${defaultAvatarIconClass}` : '';
                    previewAvatar.innerHTML = `<span class=\"material-symbols-outlined${iconClass}\">${defaultAvatarIcon}</span>`;
                }
            }
            if (avatarOptions) {
                avatarOptions.classList.remove('hidden');
            }
            if (fileInfo && fileInfo.dataset.defaultText) {
                fileInfo.textContent = fileInfo.dataset.defaultText;
            }
            if (previewWrapper) {
                previewWrapper.classList.add('hidden');
            }
            if (uploadArea) {
                uploadArea.classList.remove('hidden');
            }
        };

        const handleFile = (file) => {
            if (!file) {
                return;
            }

            const reader = new FileReader();
            reader.onload = (event) => {
                setPreview(event.target.result, file);
            };
            reader.readAsDataURL(file);
        };

        if (fileInput) {
            fileInput.addEventListener('change', (event) => {
                const file = event.target.files[0];
                if (urlInput) {
                    urlInput.value = '';
                }
                fileInput.required = initialFileRequired;
                handleFile(file);
            });
        }

        if (removeButton) {
            removeButton.addEventListener('click', () => {
                resetPreview();
            });
        }

        if (uploadArea) {
            uploadArea.addEventListener('click', () => {
                if (fileInput) {
                    fileInput.click();
                }
            });

            uploadArea.addEventListener('dragover', (event) => {
                event.preventDefault();
                uploadArea.classList.add('border-primary', 'bg-primary/5');
            });

            uploadArea.addEventListener('dragleave', () => {
                uploadArea.classList.remove('border-primary', 'bg-primary/5');
            });

            uploadArea.addEventListener('drop', (event) => {
                event.preventDefault();
                uploadArea.classList.remove('border-primary', 'bg-primary/5');
                const file = event.dataTransfer.files[0];
                if (file && file.type.startsWith('image/')) {
                    if (fileInput) {
                        const dataTransfer = new DataTransfer();
                        dataTransfer.items.add(file);
                        fileInput.files = dataTransfer.files;
                    }
                    if (urlInput) {
                        urlInput.value = '';
                    }
                    handleFile(file);
                }
            });
        }

        if (urlInput) {
            urlInput.addEventListener('input', (event) => {
                const url = event.target.value.trim();
                if (!url) {
                    if (fileInput) {
                        fileInput.required = initialFileRequired;
                    }
                    resetPreview();
                    return;
                }

                if (fileInput) {
                    fileInput.value = '';
                    fileInput.required = false;
                }

                setPreview(url);
            });
        }

        if (clearUrlButton && urlInput) {
            clearUrlButton.addEventListener('click', () => {
                urlInput.value = '';
                resetPreview();
                if (clearUrlButton.dataset.clearMessage) {
                    alert(clearUrlButton.dataset.clearMessage);
                }
            });
        }
    };

    const formatFileSize = (bytes) => {
        if (bytes === 0) return '0 KB';
        const k = 1024;
        const sizes = ['KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        const size = parseFloat((bytes / Math.pow(k, i)).toFixed(2));
        return `${size} ${sizes[i]}`;
    };

    containers.forEach(setupContainer);
})();