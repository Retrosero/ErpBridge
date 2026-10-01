import test from 'node:test';
import assert from 'node:assert/strict';
import { catalogFilters } from '../../src/ErpBridge.CentralApi/wwwroot/katalog/js/views/catalog.js';

test('catalog filters survive URL round-trip, keep zero prices and discard invalid values', () => {
    const filters = catalogFilters({ brand: ' Ege ', stock: 'in', minPrice: '0', maxPrice: '90.50', discounted: true, cartonOnly: 'false', hasImage: 'true', sort: 'price-desc' });
    assert.deepEqual(filters, { brand: 'Ege', stock: 'in', minPrice: '0', maxPrice: '90.5', discounted: 'true', cartonOnly: '', hasImage: 'true', sort: 'price-desc' });
    assert.deepEqual(catalogFilters(Object.fromEntries(new URLSearchParams(filters))), filters);
    const bad = catalogFilters({ stock: 'bad', sort: 'bad', minPrice: '-1', maxPrice: 'Infinity', discounted: 'false' });
    assert.deepEqual(bad, catalogFilters());
});
