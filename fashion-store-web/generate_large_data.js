const fs = require('fs');
const crypto = require('crypto');

function hashPassword(password) {
    const salt = crypto.randomBytes(16);
    const passwordBytes = Buffer.from(password, 'utf-8');
    const saltedPassword = Buffer.concat([passwordBytes, salt]);
    const hash = crypto.createHash('sha256').update(saltedPassword).digest();
    const result = Buffer.concat([salt, hash]);
    return result.toString('base64');
}

const sql = [];

// Helper to escape strings
const escape = (str) => str.replace(/'/g, "''");

// 1. Accounts (10)
sql.push('-- ACCOUNTS');
sql.push('TRUNCATE TABLE accounts CASCADE;');
for (let i = 1; i <= 10; i++) {
    const role = i === 1 ? 'Admin' : 'Cashier';
    const username = i === 1 ? 'admin' : `staff${i}`;
    const hash = hashPassword('admin'); 
    sql.push(`INSERT INTO accounts (username, password, role, employeename) VALUES ('${username}', '${hash}', '${role}', 'Nhân viên ${i}');`);
}

// 2. Categories (10)
sql.push('\n-- CATEGORIES');
const catNames = ['Áo thun', 'Quần Jeans', 'Váy công sở', 'Giày cao gót', 'Túi xách', 'Phụ kiện', 'Đồ lót', 'Đồ ngủ', 'Áo khoác', 'Đồ thể thao'];
catNames.forEach((name, i) => {
    sql.push(`INSERT INTO categories (name, description) VALUES ('${name}', 'Mô tả danh mục ${name}');`);
});

// 3. Suppliers (200)
sql.push('\n-- SUPPLIERS');
for (let i = 1; i <= 200; i++) {
    sql.push(`INSERT INTO suppliers (name, contactname, phone, address) VALUES ('Nhà cung cấp ${i}', 'Người đại diện ${i}', '090${i.toString().padStart(7, '0')}', 'Địa chỉ cung cấp số ${i}');`);
}

// 4. Products (300)
sql.push('\n-- PRODUCTS');
for (let i = 1; i <= 300; i++) {
    const catId = Math.floor(Math.random() * 10) + 1;
    const price = (Math.floor(Math.random() * 20) + 5) * 50000;
    const stock = Math.floor(Math.random() * 100) + 10;
    sql.push(`INSERT INTO products (name, code, categoryid, saleprice, stockquantity) VALUES ('Sản phẩm mẫu ${i}', 'PROD${i.toString().padStart(4, '0')}', ${catId}, ${price}, ${stock});`);
}

// 5. Customers (500)
sql.push('\n-- CUSTOMERS');
for (let i = 1; i <= 500; i++) {
    sql.push(`INSERT INTO customers (name, phone, address) VALUES ('Khách hàng ${i}', '098${i.toString().padStart(7, '0')}', 'Thành phố ${i}');`);
}

// 6. Invoices (2000) & Items
sql.push('\n-- INVOICES');
for (let i = 1; i <= 2000; i++) {
    const custId = Math.floor(Math.random() * 500) + 1;
    const empId = Math.floor(Math.random() * 10) + 1;
    const itemCount = Math.floor(Math.random() * 3) + 1;
    const invoiceNum = `INV-SAMPLE-${i.toString().padStart(5, '0')}`;
    
    let subtotal = 0;
    const itemsSql = [];
    
    for(let j = 0; j < itemCount; j++) {
        const prodId = Math.floor(Math.random() * 300) + 1;
        const qty = Math.floor(Math.random() * 3) + 1;
        const price = (Math.floor(Math.random() * 20) + 5) * 50000;
        const lineTotal = price * qty;
        subtotal += lineTotal;
        itemsSql.push(`INSERT INTO invoiceitems (invoiceid, productid, employeeid, unitprice, quantity, linetotal) VALUES (currval('invoices_id_seq'), ${prodId}, ${empId}, ${price}, ${qty}, ${lineTotal});`);
    }
    
    const tax = subtotal * 0.08;
    const total = subtotal + tax;
    
    sql.push(`INSERT INTO invoices (invoicenumber, customerid, employeeid, subtotal, taxamount, total, paid) VALUES ('${invoiceNum}', ${custId}, ${empId}, ${subtotal}, ${tax}, ${total}, ${total});`);
    sql.push(...itemsSql);
}

fs.writeFileSync('large_sample_data.sql', sql.join('\n'), 'utf8');
console.log('✅ Done! 2000 invoices generated in large_sample_data.sql');
