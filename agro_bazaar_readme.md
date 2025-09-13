# 🌾 AgroBazaar – A Farmer’s Digital Marketplace  

AgroBazaar is a **digital platform that connects farmers directly with consumers**, eliminating middlemen and ensuring **fair prices, transparency, and trust**.  
It empowers farmers to showcase their produce, while consumers get access to **fresh, organic, and affordable products** delivered directly from the source.  

---

## 🚀 Features  

### 👩‍🌾 For Farmers  
- **Direct Marketplace Access** – List and sell produce directly to consumers.  
- **Fair Pricing** – Farmers decide the price without middlemen interference.  
- **Digital Profile** – Showcase farm details, certifications, and products.  
- **Real-Time Orders** – Receive notifications for new orders instantly.  
- **Analytics Dashboard** – Insights on demand, sales trends, and consumer preferences.  

### 🛒 For Consumers  
- **Browse Fresh Produce** – Access organic, locally grown fruits, vegetables, and grains.  
- **Fair & Transparent Pricing** – Buy directly from farmers without hidden costs.  
- **Location-Based Search** – Find farmers nearby for faster delivery.  
- **Secure Payments** – Multiple payment options including UPI, cards, and wallets.  
- **Delivery Tracking** – Real-time updates on your orders.  

### 🌐 Platform-Wide  
- **OTP-based Signup/Login** – Secure and verified onboarding.  
- **Google & GitHub OAuth** – Quick signup with trusted providers.  
- **JWT-Based Authentication** – Secure sessions with role-based access.  
- **AI-Powered Recommendations (Future Scope)** – Suggests the best produce for consumers & demand trends for farmers.  
- **Multi-Language Support** – Breaking language barriers for accessibility.  

---

## 🏗️ Tech Stack  

- **Frontend:** React.js, Tailwind CSS / Bootstrap  
- **Backend:** Node.js (Express.js)  
- **Database:** MongoDB / MySQL  
- **Authentication:** JWT, Google OAuth, GitHub OAuth  
- **Payments:** Razorpay / Stripe integration  
- **Deployment:** Docker, AWS / Vercel  

---

## ⚙️ Installation & Setup  

Follow these steps to run AgroBazaar locally:  

### 1️⃣ Clone the Repository  
```bash
git clone https://github.com/your-username/AgroBazaar.git
cd AgroBazaar
```

### 2️⃣ Backend Setup  
```bash
cd backend
npm install
```

Create a `.env` file in the `backend` folder with the following:  
```env
PORT=5000
MONGO_URI=your_mongodb_connection_string
JWT_SECRET=your_secret_key
GOOGLE_CLIENT_ID=your_google_client_id
GOOGLE_CLIENT_SECRET=your_google_client_secret
GITHUB_CLIENT_ID=your_github_client_id
GITHUB_CLIENT_SECRET=your_github_client_secret
```

Run the backend:  
```bash
npm run dev
```

### 3️⃣ Frontend Setup  
```bash
cd ../frontend
npm install
npm start
```

### 4️⃣ Access the Application  
Open your browser and visit:  
```
http://localhost:3000
```

---

## 📌 Future Enhancements  
- AI-based crop demand prediction.  
- Blockchain-enabled transparent pricing.  
- Integration with logistics partners for deliveries.  
- Farmer loan/insurance support system.  
- Community forum for farmers and consumers.  

---

## 🤝 Contributing  
Contributions are welcome! Please fork this repository and submit a pull request.  

---

## 📜 License  
This project is licensed under the **MIT License**.  

---

## 👨‍💻 Authors  
- **Manthan Parekh** – Developer  
- Special Thanks to **Apurva Mehta (Swiggy Co-Founder)** for inspiration.