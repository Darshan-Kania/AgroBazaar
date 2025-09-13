CREATE TABLE `Users` (
  `user_id` int PRIMARY KEY AUTO_INCREMENT,
  `name` varchar(255),
  `email` varchar(255) UNIQUE,
  `phone` varchar(255),
  `password_hash` varchar(255),
  `role` varchar(255),
  `address` text,
  `created_at` datetime
);

CREATE TABLE `Products` (
  `product_id` int PRIMARY KEY AUTO_INCREMENT,
  `user_id` int,
  `name` varchar(255),
  `description` text,
  `category` varchar(255),
  `price` decimal,
  `stock` int,
  `unit` varchar(255),
  `created_at` datetime
);

CREATE TABLE `Cart` (
  `cart_id` int PRIMARY KEY AUTO_INCREMENT,
  `user_id` int,
  `created_at` datetime
);

CREATE TABLE `CartItems` (
  `cart_item_id` int PRIMARY KEY AUTO_INCREMENT,
  `cart_id` int,
  `product_id` int,
  `quantity` int
);

CREATE TABLE `Orders` (
  `order_id` int PRIMARY KEY AUTO_INCREMENT,
  `user_id` int,
  `total_amount` decimal,
  `status` varchar(255),
  `created_at` datetime
);

CREATE TABLE `OrderItems` (
  `order_item_id` int PRIMARY KEY AUTO_INCREMENT,
  `order_id` int,
  `product_id` int,
  `quantity` int,
  `price` decimal
);

CREATE TABLE `Payments` (
  `payment_id` int PRIMARY KEY AUTO_INCREMENT,
  `order_id` int,
  `amount` decimal,
  `payment_method` varchar(255),
  `payment_status` varchar(255),
  `transaction_id` varchar(255),
  `created_at` datetime
);

CREATE TABLE `DeliveryTracking` (
  `tracking_id` int PRIMARY KEY AUTO_INCREMENT,
  `order_id` int,
  `status` varchar(255),
  `updated_at` datetime
);

ALTER TABLE `Products` ADD FOREIGN KEY (`user_id`) REFERENCES `Users` (`user_id`);

ALTER TABLE `Cart` ADD FOREIGN KEY (`user_id`) REFERENCES `Users` (`user_id`);

ALTER TABLE `CartItems` ADD FOREIGN KEY (`cart_id`) REFERENCES `Cart` (`cart_id`);

ALTER TABLE `CartItems` ADD FOREIGN KEY (`product_id`) REFERENCES `Products` (`product_id`);

ALTER TABLE `Orders` ADD FOREIGN KEY (`user_id`) REFERENCES `Users` (`user_id`);

ALTER TABLE `OrderItems` ADD FOREIGN KEY (`order_id`) REFERENCES `Orders` (`order_id`);

ALTER TABLE `OrderItems` ADD FOREIGN KEY (`product_id`) REFERENCES `Products` (`product_id`);

ALTER TABLE `Payments` ADD FOREIGN KEY (`order_id`) REFERENCES `Orders` (`order_id`);

ALTER TABLE `DeliveryTracking` ADD FOREIGN KEY (`order_id`) REFERENCES `Orders` (`order_id`);
