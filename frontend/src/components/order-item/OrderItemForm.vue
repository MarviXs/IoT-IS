<template>
    <q-form @submit.prevent="onSubmit">
        <q-card-section class="q-pt-none column q-gutter-md">
            <!-- Select pre výber produktu -->
            <q-select v-model="orderItem.productId" :options="productOptions" :loading="loadingProducts"
                label="Select Product" option-value="id" option-label="code" outlined dense use-input
                input-debounce="300" emit-value map-options @popup-show="loadDefaultProducts" @filter="filterProducts"
                @update:model-value="onProductSelected">
                <template v-slot:no-option>
                    <q-item>
                        <q-item-section class="text-grey">No products found</q-item-section>
                    </q-item>
                </template>
            </q-select>

            <!-- Pole pre množstvo -->
            <q-input v-model="orderItem.quantity" label="Quantity" type="number" :rules="quantityRules" outlined
                dense />
        </q-card-section>

        <q-card-actions align="right" class="q-pt-md">
            <q-btn flat label="Cancel" color="negative" @click="cancel" />
            <q-btn unelevated label="Save" color="primary" type="submit" :loading="loading" />
        </q-card-actions>
    </q-form>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue';
import ProductService from '@/api/services/ProductService';

const props = defineProps({
    orderId: Number,
    orderItem: {
        type: Object,
        required: true,
    },
    loading: Boolean,
});

const emit = defineEmits(['on-submit', 'update:orderItem']);

// 🛒 Premenné pre produkty
const productOptions = ref([]);
const loadingProducts = ref(false);
const allProductsLoaded = ref(false); // Flag pre načítanie všetkých produktov

// 🛡️ Pravidlá validácie pre množstvo
const quantityRules = [(val) => (val && val > 0) || 'Quantity is required'];

// Sledujeme orderId a aktualizujeme orderItem
watch(() => props.orderId, (newValue) => {
    props.orderItem.orderId = newValue;
});

// 🟢 **1. Načítanie všetkých produktov pri otvorení selectu**
async function loadDefaultProducts() {
    if (allProductsLoaded.value) return; // Ak už boli produkty načítané, nerob nič

    await fetchProducts(); // Načítanie všetkých produktov
    allProductsLoaded.value = true; // Nastavíme flag
}

// 🟡 **2. Dynamické filtrovanie produktov podľa textu**
async function filterProducts(val, update) {
    if (!val) {
        update(() => {
            productOptions.value = productOptions.value;
        });
        return;
    }

    await fetchProducts(val);
    update(() => {
        productOptions.value = productOptions.value;
    });
}

// 🔵 **3. Fetch produkty (všetky alebo filtrované)**
async function fetchProducts(searchTerm = '') {
    try {
        loadingProducts.value = true;
        const queryParams = {
            SearchTerm: searchTerm,
            PageNumber: 1,
            PageSize: 20,
        };

        const response = await ProductService.getProducts(queryParams);

        if (response?.data?.items) {
            productOptions.value = response.data.items.map((product) => ({
                id: product.id,
                code: product.code || product.pluCode || 'Unnamed Product',
            }));
        } else {
            productOptions.value = [];
        }
    } catch (error) {
        console.error('Failed to fetch products:', error);
        productOptions.value = [];
    } finally {
        loadingProducts.value = false;
    }
}

// 🟣 **4. Aktualizácia objektu po výbere produktu**
function onProductSelected(productId) {
    const selectedProduct = productOptions.value.find((p) => p.id === productId);
    emit('update:orderItem', {
        ...props.orderItem,
        productId: selectedProduct?.id || '',
        name: selectedProduct?.code || 'Unnamed Product',
    });
}

// ⚫ **5. Odoslanie formulára**
function onSubmit() {
    emit('on-submit');
}

// 🔴 **6. Zrušenie formulára**
function cancel() {
    emit('update:orderItem', null);
}
</script>

<style scoped>
.q-card-section {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.q-select {
    z-index: 9999 !important;
}
</style>
