<template>
  <div class="d-flex flex-column ga-4">
    <v-card variant="tonal" class="pa-4">
      <div class="d-flex flex-column ga-3 flex-sm-row align-sm-center">
        <FileInput
          v-model="uploadFiles"
          :accept="['image/png', 'image/jpeg']"
          multiple
        />

        <div class="d-flex ga-2 flex-wrap">
          <v-tooltip
            :disabled="!isUploadDisabled"
            location="top"
            :text="t('gallery.uploadDisabledHint')"
          >
            <template #activator="{ props }">
              <span
                v-bind="props"
                class="button-hover-target"
                :class="{ 'button-hover-target--disabled': isUploadDisabled }"
              >
                <v-btn
                  color="primary"
                  variant="elevated"
                  :disabled="isUploadDisabled"
                  @click="uploadImagesAsync"
                >
                  {{ t("gallery.upload") }}
                </v-btn>
              </span>
            </template>
          </v-tooltip>
          <v-btn
            color="secondary"
            variant="outlined"
            @click="loadImageGalleryAsync"
          >
            {{ t("gallery.reload") }}
          </v-btn>
        </div>
      </div>
    </v-card>

    <v-row v-if="imageGallery.images.length > 0" dense>
      <v-col
        v-for="(image, index) in imageGallery.images"
        :key="`${image.url}-${index}`"
        cols="12"
        sm="6"
        md="4"
        lg="3"
      >
        <ImageGalleryItem
          :image="image"
          :index="index"
          @delete="deleteImageAsync"
        />
      </v-col>
    </v-row>

    <v-empty-state
      v-else
      :headline="t('gallery.emptyHeadline')"
      :title="t('gallery.emptyTitle')"
      :text="t('gallery.emptyText')"
    />
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import FileInput from "@/components/FileInput.vue";
import ImageGalleryItem from "@/components/ImageGalleryItem.vue";

interface Image {
  url: string;
  name: string;
}

interface ImageGalleryResult {
  images: Image[];
}

const uploadFiles = ref<File[]>([]);
const isUploadDisabled = computed(() => uploadFiles.value.length === 0);
const { t } = useI18n();

const imageGallery = ref<ImageGalleryResult>({
  images: [],
});

async function uploadImagesAsync() {
  if (uploadFiles.value.length > 0) {
    try {
      const formData = new FormData();

      uploadFiles.value.forEach((file) => {
        formData.append(`file[${file.name}]`, file);
      });

      const response = await fetch("image/upload", {
        method: "POST",
        body: formData,
      });

      if (!response.ok) {
        alert(response.statusText);
      }
    } catch (error) {
      alert(error);
    }
  }

  uploadFiles.value = [];
  await loadImageGalleryAsync();
}

async function getImageGalleryAsync() {
  try {
    const response = await fetch("/image-gallery");

    if (response.ok) {
      return (await response.json()) as ImageGalleryResult;
    }

    throw new Error(response.statusText);
  } catch (error) {
    alert(error);

    return {
      images: [],
    };
  }
}

async function deleteImageAsync(index: number) {
  try {
    const image = imageGallery.value.images[index];
    const imageFullName = image.url.split("/").pop();
    const response = await fetch(`/image/delete/${imageFullName}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error(response.statusText);
    }
  } catch (error) {
    alert(error);
  } finally {
    await loadImageGalleryAsync();
  }
}

async function loadImageGalleryAsync() {
  imageGallery.value = await getImageGalleryAsync();
}

onMounted(loadImageGalleryAsync);
</script>

<style scoped>
.button-hover-target {
  display: inline-flex;
}

.button-hover-target--disabled {
  cursor: not-allowed;
}
</style>
