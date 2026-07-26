<template>
  <input
    ref="input"
    type="file"
    :accept="accept"
    @change="onFileInputChange"
    :multiple="props.multiple"
  />
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";

type FileInputModel = File[] | null;

interface FileInputProps {
  modelValue: FileInputModel;
  accept?: string[];
  multiple?: boolean;
}

interface FileInputEmits {
  "update:modelValue": [v: FileInputModel];
}

const props = withDefaults(defineProps<FileInputProps>(), {
  multiple: false,
});
const emit = defineEmits<FileInputEmits>();

const input = ref<HTMLInputElement>();
const modelValueWrapper = computed(() => props.modelValue);
const accept = computed(() => {
  if (Array.isArray(props.accept)) {
    if (props.accept.length > 1) {
      return props.accept.reduce(
        (previousValue, currentValue) => `${previousValue}, ${currentValue}`,
      );
    }

    if (props.accept.length === 1) {
      return props.accept[0];
    }
  }

  return "*";
});

watch(
  modelValueWrapper,
  (newVal) => {
    if (input.value) {
      // Clear the input if modelValue is null or empty
      if (!newVal || newVal.length === 0) {
        input.value.value = "";
      }
      // Note: You cannot programmatically set files on an <input type="file"> for security reasons.
      // Browsers do not allow setting input.value or input.files to a File[].
      // The best you can do is clear the input as above.
    }
  },
  {
    immediate: true,
  },
);

function onFileInputChange(event: Event) {
  const inputElement = event.target as HTMLInputElement;
  const files = inputElement.files;

  if (files) {
    if (files.length === 0) {
      emit("update:modelValue", null);
    } else {
      emit("update:modelValue", Array.from(files));
    }
  }
}
</script>
