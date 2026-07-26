<template>
  <v-card class="h-100" elevation="4" rounded="lg">
    <v-img :src="image.url" :alt="displayName" height="220" cover />

    <v-card-title class="text-subtitle-1 text-truncate" :title="displayName">
      {{ displayName }}
    </v-card-title>

    <v-card-actions>
      <v-spacer />
      <v-btn color="error" variant="elevated" @click="$emit('delete', index)">
        {{ t("gallery.delete") }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";

interface ImageGalleryItem {
  url: string;
  name: string;
}

interface ImageGalleryItemProps {
  image: ImageGalleryItem;
  index: number;
}

interface ImageGalleryItemEmits {
  delete: [index: number];
}

const props = defineProps<ImageGalleryItemProps>();
defineEmits<ImageGalleryItemEmits>();
const { t } = useI18n();

const displayName = computed(() => {
  if (props.image.name?.trim()) {
    return props.image.name;
  }

  return props.image.url.split("/").pop() ?? "Image";
});
</script>
