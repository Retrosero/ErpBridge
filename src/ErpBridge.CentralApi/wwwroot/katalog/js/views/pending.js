// Placeholder for the screens that land with W3 (cart, orders) and W4 (statement, invoices,
// purchased, password), so every route in GOAL_MUSTERI_KATALOGU §7 already resolves.

import { h } from '../dom.js';
import { count } from '../format.js';
import { routePath } from '../route-parse.js';
import { emptyState } from '../ui/components.js';

const TITLES = {
    cart: 'Sepet',
    orders: 'Siparişlerim',
    order: 'Sipariş',
    statement: 'Hesap ekstresi',
    invoices: 'Faturalarım',
    invoice: 'Fatura',
    purchased: 'Daha önce aldıklarım',
    password: 'Şifre değiştir',
};

export function pendingView(ctx) {
    const name = ctx.route.name;
    const lines = ctx.store.get('cart') || [];
    const text = name === 'cart' && lines.length
        ? 'Sepetinizde ' + count(lines.length) + ' ürün var; sipariş talebi gönderme yakında bu ekranda açılacak.'
        : 'Bu bölüm yakında açılacak.';
    const node = emptyState({
        icon: name === 'cart' ? 'cart' : 'orders',
        title: TITLES[name] || 'Katalog',
        text,
        action: h('a', { class: 'app-btn app-btn--secondary btn-touch', href: routePath(ctx.code, 'catalog') }, 'Kataloğa dön'),
    });
    return { name, title: TITLES[name] || 'Katalog', node };
}
