-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Jun 04, 2026 at 01:07 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `pms_system`
--

-- --------------------------------------------------------

--
-- Table structure for table `activity_log`
--

CREATE TABLE `activity_log` (
  `ActivityID` int(11) NOT NULL,
  `ActivityType` varchar(50) DEFAULT NULL,
  `Description` text DEFAULT NULL,
  `CreatedAt` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `activity_log`
--

INSERT INTO `activity_log` (`ActivityID`, `ActivityType`, `Description`, `CreatedAt`) VALUES
(1, 'Giriş', '301 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-16 12:41:44'),
(2, 'Giriş', '302 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-16 12:42:22'),
(3, 'Giriş', '102 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-19 13:26:41'),
(4, 'Ödeme', '20 nolu rezervasyon için £500.00 tutarında ödeme alındı (Nakit/Kredi (Kısmi)).', '2026-04-19 13:27:18'),
(5, 'Ödeme', '7 nolu rezervasyon için £400.00 tutarında ödeme alındı (Nakit/Kredi (Kısmi)).', '2026-04-19 13:52:10'),
(6, 'Ödeme', '7 nolu rezervasyon için £219,024,610.00 tutarında ödeme alındı (💳 Kredi Kartı (USD - Final)).', '2026-04-19 14:11:50'),
(7, 'Çıkış', '104 nolu oda boşaltıldı (Rezervasyon ID: 7).', '2026-04-19 14:11:50'),
(8, 'Ödeme', '20 nolu rezervasyon için £500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-20 00:05:51'),
(9, 'Ödeme', '11 nolu rezervasyon için £400.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-20 00:07:24'),
(10, 'Ödeme', '10 nolu rezervasyon için £17,879.56 tutarında ödeme alındı (💵 Nakit Ödeme (USD)).', '2026-04-20 00:17:08'),
(11, 'Ödeme', '11 nolu rezervasyon için £250.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-20 09:30:19'),
(12, 'Ödeme', '11 nolu rezervasyon için £100.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-20 09:30:44'),
(13, 'Ödeme', '11 nolu rezervasyon için £60.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-20 09:31:38'),
(14, 'Ödeme', '15 nolu rezervasyon için £980.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-20 09:32:38'),
(15, 'Çıkış', '103 nolu oda boşaltıldı (Rezervasyon ID: 15).', '2026-04-20 09:32:38'),
(16, 'Ödeme', '11 nolu rezervasyon için £110.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-20 09:37:19'),
(17, 'Ödeme', '13 nolu rezervasyon için £2,000.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-20 09:38:45'),
(18, 'Ödeme', '11 nolu rezervasyon için £120.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-20 13:51:27'),
(19, 'Ödeme', '11 nolu rezervasyon için £0.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-20 13:51:38'),
(20, 'Çıkış', '106 nolu oda boşaltıldı (Rezervasyon ID: 11).', '2026-04-20 13:51:38'),
(21, 'Ödeme', '13 nolu rezervasyon için £0.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-20 13:51:44'),
(22, 'Çıkış', '201 nolu oda boşaltıldı (Rezervasyon ID: 13).', '2026-04-20 13:51:44'),
(23, 'Ödeme', '14 nolu rezervasyon için £1,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-20 13:51:54'),
(24, 'Çıkış', '201 nolu oda boşaltıldı (Rezervasyon ID: 14).', '2026-04-20 13:51:54'),
(25, 'Ödeme', '10 nolu rezervasyon için £0.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-20 13:52:06'),
(26, 'Çıkış', '105 nolu oda boşaltıldı (Rezervasyon ID: 10).', '2026-04-20 13:52:06'),
(27, 'Ödeme', '16 nolu rezervasyon için £1,200.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-20 13:52:21'),
(28, 'Çıkış', '203 nolu oda boşaltıldı (Rezervasyon ID: 16).', '2026-04-20 13:52:21'),
(29, 'Ödeme', '17 nolu rezervasyon için £1,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-20 13:53:03'),
(30, 'Çıkış', '101 nolu oda boşaltıldı (Rezervasyon ID: 17).', '2026-04-20 13:53:03'),
(31, 'Ödeme', '19 nolu rezervasyon için £1,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-20 13:53:12'),
(32, 'Çıkış', '302 nolu oda boşaltıldı (Rezervasyon ID: 19).', '2026-04-20 13:53:12'),
(33, 'Ödeme', '20 nolu rezervasyon için £500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-20 13:53:16'),
(34, 'Çıkış', '102 nolu oda boşaltıldı (Rezervasyon ID: 20).', '2026-04-20 13:53:16'),
(35, 'Giriş', '101 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-20 13:53:58'),
(36, 'Giriş', '102 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-20 13:54:18'),
(37, 'Ödeme', '21 nolu rezervasyon için £620.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-20 13:55:04'),
(38, 'Ödeme', '21 nolu rezervasyon için £500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-20 13:55:33'),
(39, 'Ödeme', '18 nolu rezervasyon için £1,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-24 18:27:20'),
(40, 'Çıkış', '301 nolu oda boşaltıldı (Rezervasyon ID: 18).', '2026-04-24 18:27:20'),
(41, 'Ödeme', '22 nolu rezervasyon için £1,700.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-24 18:30:29'),
(42, 'Çıkış', '102 nolu oda boşaltıldı (Rezervasyon ID: 22).', '2026-04-24 18:30:29'),
(43, 'Giriş', '102 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-24 18:48:29'),
(44, 'Giriş', '103 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-24 19:10:38'),
(45, 'Giriş', '104 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-24 19:16:53'),
(46, 'Giriş', '105 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-24 19:31:18'),
(47, 'Ödeme', '23 nolu rezervasyon için £10,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-24 19:42:49'),
(48, 'Çıkış', '102 nolu oda boşaltıldı (Rezervasyon ID: 23).', '2026-04-24 19:42:49'),
(49, 'Ödeme', '26 nolu rezervasyon için £800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-24 19:42:54'),
(50, 'Çıkış', '105 nolu oda boşaltıldı (Rezervasyon ID: 26).', '2026-04-24 19:42:54'),
(51, 'Ödeme', '24 nolu rezervasyon için £800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-24 19:43:02'),
(52, 'Çıkış', '103 nolu oda boşaltıldı (Rezervasyon ID: 24).', '2026-04-24 19:43:02'),
(53, 'Ödeme', '25 nolu rezervasyon için £800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-24 19:43:08'),
(54, 'Çıkış', '104 nolu oda boşaltıldı (Rezervasyon ID: 25).', '2026-04-24 19:43:08'),
(55, 'Giriş', '104 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-24 19:51:44'),
(56, 'Ödeme', '27 nolu rezervasyon için £500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-24 19:52:43'),
(57, 'Ödeme', '27 nolu rezervasyon için £200.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-24 19:53:01'),
(58, 'Ödeme', '27 nolu rezervasyon için £500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-24 19:53:32'),
(59, 'Giriş', '102 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-24 19:55:27'),
(60, 'Giriş', '107 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-24 20:03:14'),
(61, 'Ödeme', '29 nolu rezervasyon için £800.00 tutarında ödeme alındı (Girişte Nakit (TL)).', '2026-04-24 20:03:14'),
(62, 'Ödeme', '29 nolu rezervasyon için £960.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-26 16:07:44'),
(63, 'Çıkış', '107 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 29).', '2026-04-26 16:07:44'),
(64, 'Oda Durumu', '208 nolu oda durumu Dirty olarak güncellendi.', '2026-04-27 03:10:31'),
(65, 'Oda Durumu', '208 nolu oda durumu Available olarak güncellendi.', '2026-04-27 03:10:39'),
(66, 'Oda Durumu', '107 nolu oda durumu Available olarak güncellendi.', '2026-04-27 03:10:48'),
(67, 'Ödeme', '21 nolu rezervasyon için £9,580.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-27 03:16:36'),
(68, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 21).', '2026-04-27 03:16:36'),
(69, 'TEMİZLİK', 'Oda 102 durumu Cleaning yapıldı.', '2026-04-27 20:56:13'),
(70, 'TEMİZLİK', 'Oda 102 durumu Available yapıldı.', '2026-04-27 21:02:58'),
(71, 'TEMİZLİK', 'Oda 101 durumu Available yapıldı.', '2026-04-27 21:03:04'),
(72, 'TEMİZLİK', 'Oda 101 durumu Available yapıldı.', '2026-04-27 21:03:05'),
(73, 'TEMİZLİK', 'Oda 101 durumu Available yapıldı.', '2026-04-27 21:03:05'),
(74, 'Giriş', '101 nolu odaya müşteri girişi yapıldı (2. yatak).', '2026-04-27 21:07:42'),
(75, 'Ödeme', '28 nolu rezervasyon için £4,620.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-27 21:14:30'),
(76, 'Çıkış', '102 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 28).', '2026-04-27 21:14:30'),
(77, 'TEMİZLİK', 'Oda 102 durumu Cleaning yapıldı.', '2026-04-29 16:04:48'),
(78, 'TEMİZLİK', 'Oda 102 durumu Available yapıldı.', '2026-04-29 16:06:41'),
(79, 'Ödeme', '30 nolu rezervasyon için £3,290.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-29 16:07:37'),
(80, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 30).', '2026-04-29 16:07:37'),
(81, 'Giriş', '102 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-29 16:10:40'),
(82, 'Giriş', '307 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-29 16:12:05'),
(83, 'Ödeme', '31 nolu rezervasyon için £500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-29 16:13:49'),
(84, 'Ödeme', '27 nolu rezervasyon için £2,390.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-29 16:14:27'),
(85, 'Ödeme', '27 nolu rezervasyon için £1,160.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-04-29 16:15:25'),
(86, 'Ödeme', '27 nolu rezervasyon için £0.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-04-29 16:16:00'),
(87, 'Çıkış', '104 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 27).', '2026-04-29 16:16:00'),
(88, 'Giriş', '103 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-04-29 22:03:11'),
(89, 'Oda Durumu', '101 nolu oda durumu Available olarak güncellendi.', '2026-04-29 22:51:25'),
(90, 'Oda Durumu', '104 nolu oda durumu Available olarak güncellendi.', '2026-04-29 22:51:29'),
(91, 'Ödeme', '33 nolu rezervasyon için £41,371.66 tutarında ödeme alındı (💳 Kredi Kartı (USD)).', '2026-05-03 13:12:44'),
(92, 'Ödeme', '33 nolu rezervasyon için £0.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-03 13:13:01'),
(93, 'Çıkış', '103 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 33).', '2026-05-03 13:13:01'),
(94, 'TEMİZLİK', 'Oda 103 durumu Available yapıldı.', '2026-05-03 22:18:52'),
(95, 'Ödeme', '31 nolu rezervasyon için £410.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY)).', '2026-05-04 09:41:13'),
(96, 'Giriş', '101 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-05-04 09:48:27'),
(97, 'Giriş', '103 nolu odaya müşteri girişi yapıldı (1. yatak).', '2026-05-04 09:51:23'),
(98, 'Rezervasyon', '104 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 02.06 - 03.06', '2026-05-04 10:00:35'),
(99, 'Rezervasyon', '205 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 02.06 - 03.06', '2026-05-04 10:01:00'),
(100, 'Rezervasyon', '109 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 02.06 - 03.06', '2026-05-04 10:01:47'),
(101, 'Giriş', '109 nolu odaya müşteri giriş işlemi yapıldı (2. yatak). Tarih: 04.05 - 04.06', '2026-05-04 10:02:31'),
(102, 'Ödeme', '35 nolu rezervasyon için £4,800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-04 10:23:51'),
(103, 'Çıkış', '103 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 35).', '2026-05-04 10:23:51'),
(104, 'Ödeme', '39 nolu rezervasyon için £24,800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-04 10:23:58'),
(105, 'Çıkış', '109 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 39).', '2026-05-04 10:23:58'),
(106, 'Ödeme', '34 nolu rezervasyon için £0.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-04 10:24:05'),
(107, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 34).', '2026-05-04 10:24:05'),
(108, 'Ödeme', '32 nolu rezervasyon için £4,250.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-04 10:24:14'),
(109, 'Çıkış', '307 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 32).', '2026-05-04 10:24:14'),
(110, 'Ödeme', '31 nolu rezervasyon için £7,000.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-04 10:24:25'),
(111, 'Çıkış', '102 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 31).', '2026-05-04 10:24:25'),
(112, 'Oda Durumu', '101 nolu oda durumu Available olarak güncellendi.', '2026-05-04 10:24:33'),
(113, 'Oda Durumu', '102 nolu oda durumu Available olarak güncellendi.', '2026-05-04 10:24:35'),
(114, 'Oda Durumu', '103 nolu oda durumu Available olarak güncellendi.', '2026-05-04 10:24:38'),
(115, 'Oda Durumu', '109 nolu oda durumu Available olarak güncellendi.', '2026-05-04 10:24:40'),
(116, 'Oda Durumu', '307 nolu oda durumu Available olarak güncellendi.', '2026-05-04 10:24:45'),
(117, 'Rezervasyon', '101 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 01.06 - 05.06', '2026-05-04 10:25:11'),
(118, 'Giriş', '101 nolu odaya müşteri giriş işlemi yapıldı (2. yatak). Tarih: 04.05 - 02.06', '2026-05-04 10:25:43'),
(119, 'Giriş', '104 nolu odaya müşteri giriş işlemi yapıldı (2. yatak). Tarih: 04.05 - 03.06', '2026-05-04 10:26:54'),
(120, 'Giriş', '109 nolu odaya müşteri giriş işlemi yapıldı (2. yatak). Tarih: 04.05 - 04.06', '2026-05-04 10:35:43'),
(121, 'Rezervasyon', '102 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 05.05 - 05.05', '2026-05-04 10:50:55'),
(122, 'Giriş', '102 nolu odaya müşteri giriş işlemi yapıldı (2. yatak). Tarih: 04.05 - 05.05', '2026-05-04 10:51:37'),
(123, 'Rezervasyon', '103 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 05.05 - 05.05', '2026-05-04 10:52:23'),
(124, 'Rezervasyon', '107 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 01.06 - 05.06', '2026-05-04 10:53:07'),
(125, 'Rezervasyon', '107 nolu odaya müşteri rezervasyon işlemi yapıldı (2. yatak). Tarih: 05.05 - 04.06', '2026-05-04 10:59:24'),
(126, 'Giriş', '103 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 04.05 - 05.05', '2026-05-04 23:20:23'),
(127, 'Oda Durumu', '110 nolu oda durumu Dirty olarak güncellendi.', '2026-05-09 14:17:21'),
(128, 'Oda Durumu', '110 nolu oda durumu Available olarak güncellendi.', '2026-05-09 16:02:01'),
(129, 'Giriş', '203 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 09.05 - 10.05', '2026-05-09 16:03:49'),
(130, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 203 → ahmett yare farah | 09.05.26–10.05.26 | 0 ₺', '2026-05-09 16:03:49'),
(131, 'Giriş', '206 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 09.05 - 10.05', '2026-05-09 16:05:48'),
(132, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 206 → muat sari | 09.05.26–10.05.26 | 1,500 ₺', '2026-05-09 16:05:48'),
(133, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 305 → Cleaning | Personel: ahmett farah', '2026-05-10 11:07:03'),
(134, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 101 → Cleaning | Personel: ahmett farah', '2026-05-10 11:07:16'),
(135, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 101 → Cleaning | Personel: ahmett farah', '2026-05-10 11:07:23'),
(136, 'Giriş', '301 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 10.05 - 11.05', '2026-05-10 11:12:39'),
(137, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 301 → mahad farah | 10.05.26–11.05.26 | 0 ₺', '2026-05-10 11:12:39'),
(138, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 101 → Available | Personel: ahmett farah', '2026-05-11 09:22:25'),
(139, 'Ödeme', '45 nolu rezervasyon için £155.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-11 09:26:02'),
(140, 'Çıkış', '102 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 45).', '2026-05-11 09:26:02'),
(141, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 305 → Available | Personel: Belirtilmedi', '2026-05-11 09:27:44'),
(142, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 101 → Available | Personel: Belirtilmedi', '2026-05-11 09:27:51'),
(143, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 102 → Cleaning | Personel: ahmett farah', '2026-05-11 09:38:00'),
(144, 'Rezervasyon', '208 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 12.05 - 18.05', '2026-05-11 09:47:39'),
(145, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 208 → kamal gam | 12.05.26–18.05.26 | 4,800 ₺', '2026-05-11 09:47:39'),
(146, 'Giriş', '107 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 11.05 - 17.05', '2026-05-11 10:02:44'),
(147, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 107 → ahed yare muhan | 11.05.26–17.05.26 | 4,800 ₺', '2026-05-11 10:02:44'),
(148, 'Rezervasyon', '302 nolu odaya müşteri rezervasyon işlemi yapıldı (2. yatak). Tarih: 12.05 - 13.05', '2026-05-11 10:51:23'),
(149, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 302 → safıa satır | 12.05.26–13.05.26 | 1,000 ₺', '2026-05-11 10:51:23'),
(150, 'Giriş', '302 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 11.05 - 12.05', '2026-05-11 10:52:13'),
(151, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 302 → safıa satır | 11.05.26–12.05.26 | 0 ₺', '2026-05-11 10:52:13'),
(152, 'No-Show', '102 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-05-11 19:32:13'),
(153, 'No-Show', '103 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-05-11 19:32:13'),
(154, 'No-Show', '107 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-05-11 19:32:13'),
(155, 'Onay', '101 nolu oda için bekleyen online rezervasyon onaylandı ve giriş yapıldı.', '2026-05-11 19:36:00'),
(156, 'Onay', '309 nolu oda için bekleyen online rezervasyon onaylandı ve giriş yapıldı.', '2026-05-11 20:01:49'),
(157, 'Onay', '201 nolu oda için bekleyen online rezervasyon onaylandı ve giriş yapıldı.', '2026-05-11 20:05:46'),
(158, 'Onay', '204 nolu oda için bekleyen online rezervasyon onaylandı ve giriş yapıldı.', '2026-05-11 20:19:41'),
(159, 'No-Show', '208 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-05-16 15:22:56'),
(160, 'No-Show', '302 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-05-16 15:22:56'),
(161, 'Onay', '105 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-16 16:07:54'),
(162, 'Check-In', '105 nolu odaya müşteri girişi yapıldı.', '2026-05-16 16:08:16'),
(163, 'Onay', '102 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-16 16:32:52'),
(164, 'Check-In', '102 nolu odaya müşteri girişi yapıldı.', '2026-05-16 16:33:22'),
(165, 'Onay', '106 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-16 16:44:18'),
(166, 'Check-In', '106 nolu odaya müşteri girişi yapıldı.', '2026-05-16 16:45:31'),
(167, 'Onay', '210 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-16 16:56:04'),
(168, 'Check-In', '210 nolu odaya müşteri girişi yapıldı.', '2026-05-16 16:56:20'),
(169, 'Onay', '202 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-17 11:47:24'),
(170, 'Check-In', '202 nolu odaya müşteri girişi yapıldı.', '2026-05-17 11:47:45'),
(171, 'Ödeme', '57 nolu rezervasyon için £7,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:52:42'),
(172, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 57).', '2026-05-17 11:52:42'),
(173, 'Ödeme', '62 nolu rezervasyon için £1,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:53:02'),
(174, 'Çıkış', '102 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 62).', '2026-05-17 11:53:02'),
(175, 'Ödeme', '49 nolu rezervasyon için £150.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:53:22'),
(176, 'Çıkış', '103 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 49).', '2026-05-17 11:53:22'),
(177, 'Ödeme', '61 nolu rezervasyon için £800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:53:27'),
(178, 'Çıkış', '105 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 61).', '2026-05-17 11:53:27'),
(179, 'Ödeme', '63 nolu rezervasyon için £800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:53:36'),
(180, 'Çıkış', '106 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 63).', '2026-05-17 11:53:36'),
(181, 'Ödeme', '54 nolu rezervasyon için £4,800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:53:43'),
(182, 'Çıkış', '107 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 54).', '2026-05-17 11:53:43'),
(183, 'Ödeme', '50 nolu rezervasyon için £205.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:53:55'),
(184, 'Çıkış', '203 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 50).', '2026-05-17 11:53:55'),
(185, 'Ödeme', '58 nolu rezervasyon için £4,800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:54:04'),
(186, 'Çıkış', '309 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 58).', '2026-05-17 11:54:04'),
(187, 'Ödeme', '56 nolu rezervasyon için £155.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:54:13'),
(188, 'Çıkış', '302 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 56).', '2026-05-17 11:54:13'),
(189, 'Ödeme', '52 nolu rezervasyon için £0.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:54:19'),
(190, 'Çıkış', '301 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 52).', '2026-05-17 11:54:19'),
(191, 'Ödeme', '64 nolu rezervasyon için £800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:54:36'),
(192, 'Çıkış', '210 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 64).', '2026-05-17 11:54:36'),
(193, 'Ödeme', '51 nolu rezervasyon için £12,215.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-17 11:54:46'),
(194, 'Çıkış', '206 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 51).', '2026-05-17 11:54:46'),
(195, 'Oda Durumu', '102 nolu oda durumu Available olarak güncellendi.', '2026-05-17 11:54:54'),
(196, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 103 → Available | Personel: ahmett farah', '2026-05-17 11:55:11'),
(197, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 206 → Available | Personel: ahmett farah', '2026-05-17 11:55:16'),
(198, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 105 → Available | Personel: ahmett farah', '2026-05-17 11:55:18'),
(199, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 106 → Available | Personel: ahmett farah', '2026-05-17 11:55:20'),
(200, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 107 → Available | Personel: ahmett farah', '2026-05-17 11:55:23'),
(201, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 309 → Available | Personel: ahmett farah', '2026-05-17 11:55:26'),
(202, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 210 → Available | Personel: ahmett farah', '2026-05-17 11:55:33'),
(203, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 203 → Available | Personel: ahmett farah', '2026-05-17 11:55:38'),
(204, 'Oda Durumu', '301 nolu oda durumu Available olarak güncellendi.', '2026-05-17 11:55:48'),
(205, 'Oda Durumu', '302 nolu oda durumu Available olarak güncellendi.', '2026-05-17 11:55:52'),
(206, 'Onay', '102 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-18 09:08:30'),
(207, 'Giriş', '307 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 18.05 - 19.05', '2026-05-18 09:19:30'),
(208, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 307 → farobadan mohan | 18.05.26–19.05.26 | 800 ₺', '2026-05-18 09:19:30'),
(209, 'Rezervasyon', '210 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 23.05 - 01.06', '2026-05-18 09:28:54'),
(210, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 210 → muhubo  jamac | 23.05.26–01.06.26 | 7,200 ₺', '2026-05-18 09:28:54'),
(211, 'Rezervasyon', '201 nolu odaya müşteri rezervasyon işlemi yapıldı (2. yatak). Tarih: 24.05 - 01.06', '2026-05-18 09:30:01'),
(212, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 201 → yahye muhan | 24.05.26–01.06.26 | 12,000 ₺', '2026-05-18 09:30:01'),
(213, 'Onay', '108 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-18 09:33:33'),
(214, 'Giriş', '102 nolu oda için bekleyen rezervasyon onaylandı.', '2026-05-18 09:33:50'),
(215, 'Onay', '101 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-18 09:35:39'),
(216, 'Onay', '103 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-18 09:35:50'),
(217, 'Giriş', '108 nolu oda için bekleyen rezervasyon onaylandı.', '2026-05-18 09:40:19'),
(218, 'Onay', '104 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-18 09:59:45'),
(219, 'Check-In', '104 nolu odaya müşteri girişi yapıldı.', '2026-05-18 10:00:14'),
(220, 'Ödeme', '66 nolu rezervasyon için £1,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-18 10:01:14'),
(221, 'Çıkış', '202 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 66).', '2026-05-18 10:01:14'),
(222, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 202 → Available | Personel: ahmett farah', '2026-05-18 10:01:54'),
(223, 'Onay', '104 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-18 10:50:15'),
(224, 'No-Show', '101 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-05-21 16:16:58'),
(225, 'No-Show', '103 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-05-21 16:16:58'),
(226, 'No-Show', '102 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-05-21 16:16:58'),
(227, 'No-Show', '104 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-05-21 16:16:58'),
(228, 'Ödeme', '67 nolu rezervasyon için £4,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-21 16:17:38'),
(229, 'Çıkış', '102 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 67).', '2026-05-21 16:17:38'),
(230, 'Ödeme', '73 nolu rezervasyon için £2,400.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-21 16:17:56'),
(231, 'Çıkış', '104 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 73).', '2026-05-21 16:17:56'),
(232, 'Ödeme', '72 nolu rezervasyon için £2,400.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-21 16:19:06'),
(233, 'Çıkış', '108 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 72).', '2026-05-21 16:19:06'),
(234, 'Ödeme', '68 nolu rezervasyon için £2,400.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-21 16:19:14'),
(235, 'Çıkış', '307 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 68).', '2026-05-21 16:19:14'),
(236, 'Oda Durumu', '307 nolu oda durumu Available olarak güncellendi.', '2026-05-21 16:19:27'),
(237, 'Oda Durumu', '102 nolu oda durumu Available olarak güncellendi.', '2026-05-21 16:19:31'),
(238, 'Oda Durumu', '108 nolu oda durumu Available olarak güncellendi.', '2026-05-21 16:19:33'),
(239, 'Onay', '105 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-21 16:24:03'),
(240, 'Check-In', '105 nolu odaya müşteri girişi yapıldı.', '2026-05-21 16:24:22'),
(241, 'Onay', '101 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-21 16:28:38'),
(242, 'Check-In', '101 nolu odaya müşteri girişi yapıldı.', '2026-05-21 16:28:54'),
(243, 'Ödeme', '76 nolu rezervasyon için £800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-21 16:41:16'),
(244, 'Çıkış', '105 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 76).', '2026-05-21 16:41:16'),
(245, 'Ödeme', '41 nolu rezervasyon için £3,250.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-21 16:41:47'),
(246, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 41).', '2026-05-21 16:41:47'),
(247, 'Ödeme', '77 nolu rezervasyon için £1,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-21 16:42:54'),
(248, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 77).', '2026-05-21 16:42:54'),
(249, 'Oda Durumu', '101 nolu oda durumu Available olarak güncellendi.', '2026-05-21 16:43:07'),
(250, 'Oda Durumu', '105 nolu oda durumu Available olarak güncellendi.', '2026-05-21 21:40:34'),
(251, 'Giriş', '101 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 21.05 - 22.05', '2026-05-21 22:42:02'),
(252, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 101 → AHMED jamac | 21.05.26–22.05.26 | 1,000 ₺', '2026-05-21 22:42:02'),
(253, 'No-Show', '108 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-05-22 00:00:03'),
(254, 'Ödeme', '78 nolu rezervasyon için £1,000.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-22 00:00:47'),
(255, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 78).', '2026-05-22 00:00:47'),
(256, 'Oda Durumu', '101 nolu oda durumu Available olarak güncellendi.', '2026-05-22 00:00:53'),
(257, 'Onay', '101 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-22 00:07:50'),
(258, 'Check-In', '210 nolu odaya müşteri girişi yapıldı.', '2026-05-22 00:08:01'),
(259, 'Check-In', '101 nolu odaya müşteri girişi yapıldı.', '2026-05-22 00:08:04'),
(260, 'Ödeme', '80 nolu rezervasyon için £7,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-22 00:08:36'),
(261, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 80).', '2026-05-22 00:08:36'),
(262, 'Oda Durumu', '101 nolu oda durumu Available olarak güncellendi.', '2026-05-22 00:08:47'),
(263, 'Onay', '206 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-24 12:26:18'),
(264, 'Giriş', '201 nolu oda için bekleyen rezervasyon onaylandı.', '2026-05-24 12:26:46'),
(265, 'Giriş', '206 nolu oda için bekleyen rezervasyon onaylandı.', '2026-05-24 12:26:51'),
(266, 'Check-In', '107 nolu odaya müşteri girişi yapıldı.', '2026-05-24 12:27:11'),
(267, 'Check-In', '101 nolu odaya müşteri girişi yapıldı.', '2026-05-24 12:27:15'),
(268, 'Check-In', '109 nolu odaya müşteri girişi yapıldı.', '2026-05-24 12:27:18'),
(269, 'Check-In', '205 nolu odaya müşteri girişi yapıldı.', '2026-05-24 12:27:21'),
(270, 'Check-In', '104 nolu odaya müşteri girişi yapıldı.', '2026-05-24 12:27:24'),
(271, 'Onay', '307 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-24 13:02:13'),
(272, 'Check-In', '307 nolu odaya müşteri girişi yapıldı.', '2026-05-24 13:06:12'),
(273, 'Onay', '309 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-24 13:08:50'),
(274, 'Check-In', '309 nolu odaya müşteri girişi yapıldı.', '2026-05-24 13:09:04'),
(275, 'Onay', '202 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-28 13:19:16'),
(276, 'Check-In', '202 nolu odaya müşteri girişi yapıldı.', '2026-05-28 13:19:34'),
(277, 'Ödeme', '59 nolu rezervasyon için £15,090.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-28 15:50:48'),
(278, 'Çıkış', '201 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 59).', '2026-05-28 15:50:48'),
(279, 'Ödeme', '81 nolu rezervasyon için £3,200.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-28 15:50:55'),
(280, 'Çıkış', '206 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 81).', '2026-05-28 15:50:55'),
(281, 'Ödeme', '82 nolu rezervasyon için £3,200.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-28 15:51:07'),
(282, 'Çıkış', '307 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 82).', '2026-05-28 15:51:07'),
(283, 'Ödeme', '83 nolu rezervasyon için £3,200.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-28 15:51:13'),
(284, 'Çıkış', '309 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 83).', '2026-05-28 15:51:13'),
(285, 'Oda Durumu', '206 nolu oda durumu Available olarak güncellendi.', '2026-05-28 15:51:26'),
(286, 'Oda Durumu', '307 nolu oda durumu Available olarak güncellendi.', '2026-05-28 15:51:29'),
(287, 'Oda Durumu', '309 nolu oda durumu Available olarak güncellendi.', '2026-05-28 15:51:33'),
(288, 'Onay', '301 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-30 11:55:06'),
(289, 'Check-In', '301 nolu odaya müşteri girişi yapıldı.', '2026-05-30 11:55:16'),
(290, 'Giriş', '101 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 30.05 - 31.05', '2026-05-30 11:58:47'),
(291, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 101 → mart muase | 30.05.26–31.05.26 | 1,000 ₺', '2026-05-30 11:58:47'),
(292, 'Rezervasyon', '305 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 04.06 - 08.06', '2026-05-30 13:06:22'),
(293, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 305 → kamal Gaflanov | 04.06.26–08.06.26 | 3,200 ₺', '2026-05-30 13:06:22'),
(294, 'Giriş', '305 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 30.05 - 31.05', '2026-05-30 13:07:48'),
(295, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 305 → mahad farah | 30.05.26–31.05.26 | 0 ₺', '2026-05-30 13:07:48'),
(296, 'Ödeme', '84 nolu rezervasyon için £3,215.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-30 13:09:40'),
(297, 'Çıkış', '202 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 84).', '2026-05-30 13:09:40'),
(298, 'Ödeme', '60 nolu rezervasyon için £10,400.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-30 13:09:57'),
(299, 'Çıkış', '204 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 60).', '2026-05-30 13:09:57'),
(300, 'Ödeme', '47 nolu rezervasyon için £3,200.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-30 13:14:11'),
(301, 'Çıkış', '107 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 47).', '2026-05-30 13:14:11'),
(302, 'Ödeme', '70 nolu rezervasyon için £12,000.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-30 13:14:58'),
(303, 'Çıkış', '201 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 70).', '2026-05-30 13:14:58'),
(304, 'Oda Durumu', '201 nolu oda durumu Available olarak güncellendi.', '2026-05-30 13:15:11'),
(305, 'Oda Durumu', '202 nolu oda durumu Available olarak güncellendi.', '2026-05-30 13:15:14'),
(306, 'Oda Durumu', '204 nolu oda durumu Available olarak güncellendi.', '2026-05-30 13:15:16'),
(307, 'Oda Durumu', '107 nolu oda durumu Available olarak güncellendi.', '2026-05-30 13:15:20'),
(308, 'Ödeme', '40 nolu rezervasyon için £400.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-30 13:16:04'),
(309, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 40).', '2026-05-30 13:16:04'),
(310, 'Ödeme', '42 nolu rezervasyon için £24,170.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-30 13:16:46'),
(311, 'Çıkış', '104 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 42).', '2026-05-30 13:16:46'),
(312, 'Oda Durumu', '104 nolu oda durumu Available olarak güncellendi.', '2026-05-30 13:16:58'),
(313, 'Ödeme', '38 nolu rezervasyon için £0.00 tutarında ödeme alındı (💵 Nakit Ödeme (EUR - Final)).', '2026-05-30 13:18:02'),
(314, 'Çıkış', '109 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 38).', '2026-05-30 13:18:02'),
(315, 'Ödeme', '36 nolu rezervasyon için £0.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-30 13:18:07'),
(316, 'Çıkış', '104 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 36).', '2026-05-30 13:18:07'),
(317, 'Oda Durumu', '104 nolu oda durumu Available olarak güncellendi.', '2026-05-30 13:18:17'),
(318, 'Ödeme', '69 nolu rezervasyon için £7,200.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-30 13:21:28'),
(319, 'Çıkış', '210 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 69).', '2026-05-30 13:21:28'),
(320, 'Oda Durumu', '210 nolu oda durumu Available olarak güncellendi.', '2026-05-30 13:21:46'),
(321, 'Rezervasyon', '102 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 31.05 - 02.06', '2026-05-30 13:22:53'),
(322, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 102 → Ahmet Yilmaz | 31.05.26–02.06.26 | 3,000 ₺', '2026-05-30 13:22:53'),
(323, 'Giriş', '102 nolu oda için bekleyen rezervasyon onaylandı.', '2026-05-31 10:55:19'),
(324, 'Ödeme', '86 nolu rezervasyon için £1,000.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-31 10:55:27'),
(325, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 86).', '2026-05-31 10:55:27'),
(326, 'Ödeme', '85 nolu rezervasyon için £1,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-31 10:56:00'),
(327, 'Çıkış', '301 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 85).', '2026-05-31 10:56:00'),
(328, 'Ödeme', '88 nolu rezervasyon için £0.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-31 10:56:08'),
(329, 'Çıkış', '305 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 88).', '2026-05-31 10:56:08'),
(330, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 101 → Available | Personel: ahmett farah', '2026-05-31 10:56:31'),
(331, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 305 → Available | Personel: farxiyo cali', '2026-05-31 10:56:40'),
(332, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 301 → Available | Personel: salad dahir', '2026-05-31 10:57:15'),
(333, 'Onay', '101 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-31 12:30:30'),
(334, 'Onay', '204 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-31 12:30:45'),
(335, 'Onay', '101 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-31 12:30:50'),
(336, 'Onay', '101 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-31 12:30:54'),
(337, 'Onay', '101 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-31 12:30:59'),
(338, 'Check-In', '101 nolu odaya müşteri girişi yapıldı.', '2026-05-31 12:31:07'),
(339, 'Check-In', '204 nolu odaya müşteri girişi yapıldı.', '2026-05-31 12:31:11'),
(340, 'Check-In', '101 nolu odaya müşteri girişi yapıldı.', '2026-05-31 12:31:20'),
(341, 'Ödeme', '89 nolu rezervasyon için £3,000.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-31 12:32:16'),
(342, 'Çıkış', '102 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 89).', '2026-05-31 12:32:16'),
(343, 'Ödeme', '90 nolu rezervasyon için £6,000.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-31 12:32:22'),
(344, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 90).', '2026-05-31 12:32:22'),
(345, 'Ödeme', '93 nolu rezervasyon için £1,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-31 12:32:28'),
(346, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 93).', '2026-05-31 12:32:28'),
(347, 'Oda Durumu', '102 nolu oda durumu Available olarak güncellendi.', '2026-05-31 12:32:39'),
(348, 'Check-In', '101 nolu odaya müşteri girişi yapıldı.', '2026-05-31 12:33:05'),
(349, 'Check-In', '101 nolu odaya müşteri girişi yapıldı.', '2026-05-31 12:33:21'),
(350, 'Ödeme', '91 nolu rezervasyon için £6,000.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-31 12:33:37'),
(351, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 91).', '2026-05-31 12:33:37'),
(352, 'Ödeme', '92 nolu rezervasyon için £1,500.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-31 12:33:43'),
(353, 'Çıkış', '101 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 92).', '2026-05-31 12:33:43'),
(354, 'Oda Durumu', '101 nolu oda durumu Available olarak güncellendi.', '2026-05-31 12:33:51'),
(355, 'Onay', '206 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-05-31 12:34:59'),
(356, 'Check-In', '206 nolu odaya müşteri girişi yapıldı.', '2026-05-31 12:35:14'),
(357, 'Ödeme', '37 nolu rezervasyon için £950.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-05-31 14:41:55'),
(358, 'Çıkış', '205 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 37).', '2026-05-31 14:41:55'),
(359, 'Oda Durumu', '205 nolu oda durumu Available olarak güncellendi.', '2026-05-31 14:42:04'),
(360, 'Giriş', '301 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 31.05 - 01.06', '2026-05-31 14:43:45'),
(361, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 301 → mart saytir | 31.05.26–01.06.26 | 1,500 ₺', '2026-05-31 14:43:45'),
(362, 'Giriş', '105 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 01.06 - 02.06', '2026-06-01 09:21:39'),
(363, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 105 → farobadan mohan | 01.06.26–02.06.26 | 800 ₺', '2026-06-01 09:21:39'),
(364, 'Onay', '204 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-06-01 09:25:06'),
(365, 'Rezervasyon', '204 nolu odaya müşteri rezervasyon işlemi yapıldı (1. yatak). Tarih: 03.06 - 04.06', '2026-06-01 09:29:30'),
(366, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 204 → mart muase | 03.06.26–04.06.26 | 800 ₺', '2026-06-01 09:29:30'),
(367, 'Check-In', '204 nolu odaya müşteri girişi yapıldı.', '2026-06-01 09:35:55'),
(368, 'Onay', '202 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-06-01 10:01:33'),
(369, 'Onay', '102 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-06-02 10:08:01'),
(370, 'Check-In', '102 nolu odaya müşteri girişi yapıldı.', '2026-06-02 10:08:07'),
(371, 'Ödeme', '97 nolu rezervasyon için £800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-06-02 10:36:24'),
(372, 'Çıkış', '105 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 97).', '2026-06-02 10:36:24'),
(373, '[Sistem Yoneticisi] TEMİZLİK → ROOMS', 'Oda 105 → Available | Personel: ahmett farah', '2026-06-02 10:36:43'),
(374, 'Giriş', '103 nolu odaya müşteri giriş işlemi yapıldı (1. yatak). Tarih: 02.06 - 03.06', '2026-06-02 10:38:17'),
(375, '[Sistem Yoneticisi] YENİ REZ → RESERVATIONS', 'Oda 103 → ahmett yare farah | 02.06.26–03.06.26 | 0 ₺', '2026-06-02 10:38:17'),
(376, 'Onay', '301 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-06-02 10:42:20'),
(377, 'Ödeme', '98 nolu rezervasyon için £800.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-06-02 11:28:18'),
(378, 'Çıkış', '204 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 98).', '2026-06-02 11:28:18'),
(379, 'Ödeme', '94 nolu rezervasyon için £1,600.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-06-02 11:28:27'),
(380, 'Çıkış', '204 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 94).', '2026-06-02 11:28:27'),
(381, 'Ödeme', '96 nolu rezervasyon için £3,000.00 tutarında ödeme alındı (💵 Nakit Ödeme (TRY - Final)).', '2026-06-02 11:29:43'),
(382, 'Çıkış', '301 nolu oda boşaltıldı ve temizlik bekliyor (Rezervasyon ID: 96).', '2026-06-02 11:29:43'),
(383, 'Check-In', '301 nolu odaya müşteri girişi yapıldı.', '2026-06-02 11:29:58'),
(384, 'Oda Durumu', '204 nolu oda durumu Available olarak güncellendi.', '2026-06-02 11:30:19'),
(385, 'Onay', '309 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-06-02 11:31:31'),
(386, 'Onay', '102 nolu oda için online rezervasyon onaylandı (Yer Ayırtıldı).', '2026-06-02 11:32:50'),
(387, 'Check-In', '309 nolu odaya müşteri girişi yapıldı.', '2026-06-02 11:32:57'),
(388, 'Check-In', '102 nolu odaya müşteri girişi yapıldı.', '2026-06-02 11:34:44'),
(389, 'No-Show', '204 nolu oda için beklenen müşteri gelmedi. Rezervasyon No-Show olarak işaretlendi.', '2026-06-04 12:39:16');

-- --------------------------------------------------------

--
-- Table structure for table `beds`
--

CREATE TABLE `beds` (
  `BedID` int(11) NOT NULL,
  `RoomTypeID` int(11) DEFAULT NULL,
  `BedType` varchar(50) DEFAULT NULL,
  `Capacity` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `companies`
--

CREATE TABLE `companies` (
  `CompanyID` int(11) NOT NULL,
  `CompanyName` varchar(100) DEFAULT NULL,
  `TaxNumber` varchar(20) DEFAULT NULL,
  `TaxOffice` varchar(50) DEFAULT NULL,
  `Address` text DEFAULT NULL,
  `Phone` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `customers`
--

CREATE TABLE `customers` (
  `CustomerID` int(11) NOT NULL,
  `IdentityNumber` varchar(11) DEFAULT NULL,
  `UserID` int(11) DEFAULT NULL,
  `FirstName` varchar(50) NOT NULL,
  `LastName` varchar(50) NOT NULL,
  `FatherName` varchar(50) DEFAULT NULL,
  `MotherName` varchar(50) DEFAULT NULL,
  `BirthPlace` varchar(50) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Phone` varchar(20) DEFAULT NULL,
  `Address` text DEFAULT NULL,
  `RoomNumber` varchar(10) DEFAULT NULL,
  `BedNumber` int(11) DEFAULT 1,
  `Nationality` varchar(50) DEFAULT 'Türkiye',
  `Notes` text DEFAULT NULL,
  `CreatedAt` datetime DEFAULT current_timestamp(),
  `BirthDate` date DEFAULT NULL,
  `Gender` varchar(10) DEFAULT NULL,
  `Country` varchar(50) DEFAULT NULL,
  `Preferences` text DEFAULT NULL,
  `VipStatus` varchar(50) DEFAULT 'Normal',
  `Allergies` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `customers`
--

INSERT INTO `customers` (`CustomerID`, `IdentityNumber`, `UserID`, `FirstName`, `LastName`, `FatherName`, `MotherName`, `BirthPlace`, `Email`, `Phone`, `Address`, `RoomNumber`, `BedNumber`, `Nationality`, `Notes`, `CreatedAt`, `BirthDate`, `Gender`, `Country`, `Preferences`, `VipStatus`, `Allergies`) VALUES
(1, NULL, NULL, 'ahmed', 'farah', NULL, NULL, NULL, 'guray034@gmail.com', '50342564355', 'barbars maheli sk.1:canakal.merkez', '101', 1, 'Türkiye', NULL, '2026-03-23 09:37:31', NULL, NULL, NULL, NULL, 'Normal', NULL),
(2, '12224302404', NULL, 'asad', 'damaer', '', '', '', '', '548684544', '', '102', 1, 'Türkiye', NULL, '2026-03-23 10:47:15', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(3, '12345678056', NULL, 'ahmett', 'yare farah', '', '', '', '', '5345333320', '', '203', 3, 'Türkiye', NULL, '2026-03-23 10:59:11', '1990-01-01', 'Erkek', 'Turkiye', NULL, 'Normal', NULL),
(4, '20201010120', NULL, 'muat', 'sari', '', '', '', '', '53003003304555', '', '102', 2, 'Türkiye', NULL, '2026-03-27 11:38:01', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(8, '12223305655', NULL, 'van', 'hat', NULL, NULL, NULL, 'van12@gmail.tr', '5013204074', 'İstanbul/ - ', '101', 1, 'Türkiye', NULL, '2026-03-27 11:40:18', NULL, NULL, NULL, NULL, 'Normal', NULL),
(11, '34554367456', NULL, 'efe', 'sam', '', '', '', '', '56737563467', '', '102', 1, 'Türkiye', NULL, '2026-03-30 09:31:02', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(12, '44556677667', NULL, 'mark', 'fae', NULL, NULL, NULL, 'ef12@gmail.om', '5672314074', 'damar.merkez', '', 0, 'Türkiye', NULL, '2026-03-30 09:33:53', NULL, NULL, NULL, NULL, 'Normal', NULL),
(13, '64153088476', NULL, 'kamal', 'gam', '', '', '', '', '05416264074', '', '103', 1, 'Türkiye', NULL, '2026-03-30 11:01:26', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(14, '67832465124', NULL, 'salad', 'dahır', '', '', '', '', '050130205070', '', '', 0, 'Türkiye', NULL, '2026-04-05 13:01:03', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(15, '678', NULL, 'salad', 'dahır', '', '', '', '', '', '', '104', 4, 'Türkiye', NULL, '2026-04-05 13:22:18', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(16, '99462697484', NULL, 'mahad', 'farah', '', '', '', '', '', '', '104', 1, 'Türkiye', NULL, '2026-04-05 13:33:14', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(17, '10000000146', NULL, 'asıya', 'malı', '', '', '', '', '05012304075', '', '201', 0, 'Türkiye', NULL, '2026-04-06 09:30:05', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(18, '56870335564', NULL, 'yahye', 'muhan', '', '', '', '', '05763284074', '', '103', 1, 'Türkiye', NULL, '2026-04-13 09:50:31', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(19, '14000056755', NULL, 'ahed', 'yare muhan', '', '', '', '', '05013224507', '', '', 0, 'Türkiye', NULL, '2026-04-13 11:03:38', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(20, '99999462845', NULL, 'farobadan', 'mohan', '', '', '', '', '05013205050', '', '105', 1, 'Türkiye', NULL, '2026-04-15 22:29:38', '1990-01-01', 'Erkek', 'Turkiye', NULL, 'Normal', NULL),
(21, '77098070606', NULL, 'muhubo', 'jamac ısmaıl', '', '', '', '', '05013294090', '', '', 0, 'Türkiye', NULL, '2026-04-19 13:25:54', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(22, '12300097866', NULL, 'ahmett', 'yare farah', '', '', '', '', '5345333320', '', '103', 1, 'Türkiye', NULL, '2026-04-24 19:10:38', '1990-03-01', 'Erkek', 'Almanya', NULL, 'Normal', NULL),
(23, '99462698434', NULL, 'mart', 'muase', '', '', '', '', '05013204786', '', '', 0, 'Türkiye', NULL, '2026-05-11 09:52:47', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(24, '11111110134', NULL, 'safıa', 'satır', '', '', '', '', '05382402463', '', '', 0, 'Türkiye', NULL, '2026-05-11 10:50:35', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(25, '12345678901', NULL, 'Ahmet', 'Yilmaz', '', '', '', '', '05551234567', '', NULL, 1, 'Türkiye', NULL, '2026-05-11 19:35:08', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(26, '99462697563', NULL, 'YASIN AHMED', 'AHMED', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '05013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-11 20:01:18', NULL, NULL, NULL, NULL, 'Normal', NULL),
(27, '10000004566', NULL, 'YASIN ', 'AHMED', NULL, NULL, NULL, 'guray0449@gmail.com', '05013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-11 20:05:25', NULL, NULL, NULL, NULL, 'Normal', NULL),
(29, '99462598484', NULL, 'ahmedfarah ', 'mj', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '05013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-11 20:19:24', NULL, NULL, NULL, NULL, 'Normal', NULL),
(30, '99462698464', NULL, 'fahmo ', 'farah', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '05013424074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-16 16:06:48', NULL, NULL, NULL, NULL, 'Normal', NULL),
(31, '1000008945', NULL, 'muhubo', ' jamac', '', '', '', '', '05013204074', '', NULL, 1, 'Türkiye', NULL, '2026-05-16 16:32:14', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(32, '12308476446', NULL, 'özgur ', 'çalik', NULL, NULL, NULL, 'guray0449@gmail.com', '05325984084', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-16 16:55:11', NULL, NULL, NULL, NULL, 'Normal', NULL),
(33, '1000006746', NULL, 'ayan', 'farah', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-17 11:46:33', NULL, NULL, NULL, NULL, 'Normal', NULL),
(34, '1000056457', NULL, 'mart', 'saytir', '', '', '', '', '+905013204074', '', NULL, 1, 'Türkiye', NULL, '2026-05-18 09:05:45', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(35, '100000045', NULL, 'kamil', 'saytir', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-18 09:33:18', NULL, NULL, NULL, NULL, 'Normal', NULL),
(36, '10000057447', NULL, ' AHMED ', 'YASIN', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-18 09:58:47', NULL, NULL, NULL, NULL, 'Normal', NULL),
(37, '99462697868', NULL, 'Ömer ', 'Salim', NULL, NULL, NULL, 'guray0449@gmail.com', '5013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-18 10:35:19', NULL, NULL, NULL, NULL, 'Normal', NULL),
(38, '10000454467', NULL, 'AHMED', 'jamac', '', '', '', '', '+905013204074', '', NULL, 1, 'Türkiye', NULL, '2026-05-21 16:23:27', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(39, '111111111', NULL, 'Anıl', 'Kalkan', NULL, NULL, NULL, 'klatrax17@gmail.com', '05555555555', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-21 16:28:11', NULL, NULL, NULL, NULL, 'Normal', NULL),
(40, '9987563456', NULL, 'hamse', 'abdi', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '05614245090', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-21 23:59:34', NULL, NULL, NULL, NULL, 'Normal', NULL),
(44, '99462697489', NULL, 'farhiyo', 'salad', NULL, NULL, NULL, 'guray0449@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-22 00:07:38', NULL, NULL, NULL, NULL, 'Normal', NULL),
(47, '99564697646', NULL, 'gamci', 'yare', NULL, NULL, NULL, 'guray0449@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-24 13:03:48', NULL, NULL, NULL, NULL, 'Normal', NULL),
(48, '98773045878', NULL, 'kamal', 'Gaflanov', '', '', '', '', '05067407987', '', NULL, 1, 'Türkiye', NULL, '2026-05-28 13:18:46', '1990-01-01', 'Erkek', NULL, NULL, 'Normal', NULL),
(49, '99462598474', NULL, 'nunme', 'yare', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-30 11:54:45', NULL, NULL, NULL, NULL, 'Normal', NULL),
(53, '98765432101', NULL, 'Alice', 'Smith', NULL, NULL, NULL, 'alice@example.com', '5559876543', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-31 11:48:31', NULL, NULL, NULL, NULL, 'Normal', NULL),
(54, '99784567373', NULL, 'alişa ', 'damir', NULL, NULL, NULL, 'guray0449@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-05-31 12:34:39', NULL, NULL, NULL, NULL, 'Normal', NULL),
(55, '873456233', NULL, 'amin ', 'ahmed', NULL, NULL, NULL, 'guray0449@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-06-01 09:24:30', NULL, NULL, NULL, NULL, 'Normal', NULL),
(56, '11111111111', NULL, 'Ömer Selim', 'KAYA', NULL, NULL, NULL, 'papenov477@hitzcart.com', '2', NULL, NULL, 1, 'Türkiye', NULL, '2026-06-01 10:01:17', NULL, NULL, NULL, NULL, 'Normal', NULL),
(57, '9947483622', NULL, 'Darol ', 'Samir', NULL, NULL, NULL, 'guray0449@gmail.com', '501604044646', NULL, NULL, 1, 'Türkiye', NULL, '2026-06-02 10:06:54', NULL, NULL, NULL, NULL, 'Normal', NULL),
(58, '58765555554', NULL, 'YASIN AHMED', 'AHMED', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-06-02 10:41:46', NULL, NULL, NULL, NULL, 'Normal', NULL),
(59, '10000046443', NULL, 'anil ', 'kalakan', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-06-02 11:21:32', NULL, NULL, NULL, NULL, 'Normal', NULL),
(61, '100003546', NULL, 'Dede', '4455', NULL, NULL, NULL, 'dedeyare4455@gmail.com', '+905013204074', NULL, NULL, 1, 'Türkiye', NULL, '2026-06-02 11:31:00', NULL, NULL, NULL, NULL, 'Normal', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `customer_messages`
--

CREATE TABLE `customer_messages` (
  `MessageID` int(11) NOT NULL,
  `CustomerID` int(11) NOT NULL,
  `MessageText` text NOT NULL,
  `Direction` varchar(20) DEFAULT 'Incoming',
  `CreatedAt` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `employees`
--

CREATE TABLE `employees` (
  `EmployeeID` int(11) NOT NULL,
  `FirstName` varchar(50) DEFAULT NULL,
  `LastName` varchar(50) DEFAULT NULL,
  `Role` varchar(50) DEFAULT NULL,
  `Phone` varchar(20) DEFAULT NULL,
  `Salary` decimal(10,2) DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT 1,
  `HireDate` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `employees`
--

INSERT INTO `employees` (`EmployeeID`, `FirstName`, `LastName`, `Role`, `Phone`, `Salary`, `IsActive`, `HireDate`) VALUES
(1, 'farxiyo', 'cali', 'tamilik görev', '000', 30000.00, 1, '2026-04-27'),
(2, 'salad', 'dahir', 'işçi', '000', 40000.00, 1, '2026-04-27'),
(3, 'ahmett', 'farah', 'lokanata', '000', 40000.00, 1, '2026-04-27');

-- --------------------------------------------------------

--
-- Table structure for table `end_of_day_reports`
--

CREATE TABLE `end_of_day_reports` (
  `ReportID` int(11) NOT NULL,
  `ReportDate` date DEFAULT NULL,
  `TotalCash` decimal(12,2) DEFAULT 0.00,
  `TotalCreditCard` decimal(12,2) DEFAULT 0.00,
  `TotalExpenses` decimal(12,2) DEFAULT 0.00,
  `TotalRevenue` decimal(12,2) DEFAULT 0.00,
  `CompletedBy` varchar(100) DEFAULT NULL,
  `CreatedAt` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `end_of_day_reports`
--

INSERT INTO `end_of_day_reports` (`ReportID`, `ReportDate`, `TotalCash`, `TotalCreditCard`, `TotalExpenses`, `TotalRevenue`, `CompletedBy`, `CreatedAt`) VALUES
(1, '2026-04-27', 1000.00, 2500.00, 500.00, 3500.00, 'Sistem Yoneticisi', '2026-04-27 03:09:41'),
(2, '2026-04-29', 1000.00, 2500.00, 500.00, 3500.00, 'Sistem Yoneticisi', '2026-04-29 16:16:28'),
(3, '2026-05-03', 0.00, 0.00, 0.00, 0.00, 'Sistem Yoneticisi', '2026-05-03 11:50:36'),
(4, '2026-05-09', 0.00, 0.00, 0.00, 0.00, 'Sistem Yoneticisi', '2026-05-09 14:09:03');

-- --------------------------------------------------------

--
-- Table structure for table `expenses`
--

CREATE TABLE `expenses` (
  `ExpenseID` int(11) NOT NULL,
  `Title` varchar(100) DEFAULT NULL,
  `Category` varchar(50) DEFAULT NULL,
  `Amount` decimal(18,2) DEFAULT NULL,
  `ExpenseDate` datetime DEFAULT current_timestamp(),
  `Description` text DEFAULT NULL,
  `PaidBy` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `expenses`
--

INSERT INTO `expenses` (`ExpenseID`, `Title`, `Category`, `Amount`, `ExpenseDate`, `Description`, `PaidBy`) VALUES
(1, 'elektrık', 'mutfak', 400.00, '2026-04-26 16:58:45', 'işlemler yapmak sunrunda', 'derkter'),
(2, 'şapo', 'Temizlik Malzemesi', 400.00, '2026-05-31 14:45:26', '', 'Ana Kasa');

-- --------------------------------------------------------

--
-- Table structure for table `floors`
--

CREATE TABLE `floors` (
  `FloorID` int(11) NOT NULL,
  `FloorNumber` int(11) NOT NULL,
  `Description` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `floors`
--

INSERT INTO `floors` (`FloorID`, `FloorNumber`, `Description`) VALUES
(1, 1, 'Kat 1'),
(2, 2, 'Kat 2'),
(3, 3, 'Kat 3');

-- --------------------------------------------------------

--
-- Table structure for table `housekeeping_tasks`
--

CREATE TABLE `housekeeping_tasks` (
  `TaskID` int(11) NOT NULL,
  `RoomID` int(11) DEFAULT NULL,
  `AssignedTo` varchar(100) DEFAULT NULL,
  `TaskStatus` varchar(20) DEFAULT 'Pending',
  `TaskType` varchar(50) DEFAULT 'Cleaning',
  `Notes` text DEFAULT NULL,
  `CreatedAt` datetime DEFAULT current_timestamp(),
  `CompletedAt` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `maintenance_logs`
--

CREATE TABLE `maintenance_logs` (
  `LogID` int(11) NOT NULL,
  `RoomID` int(11) DEFAULT NULL,
  `FaultDescription` text DEFAULT NULL,
  `ReportedDate` datetime DEFAULT current_timestamp(),
  `ResolvedDate` datetime DEFAULT NULL,
  `TechnicianName` varchar(100) DEFAULT NULL,
  `Cost` decimal(10,2) DEFAULT 0.00,
  `Status` varchar(20) DEFAULT 'Pending'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `maintenance_logs`
--

INSERT INTO `maintenance_logs` (`LogID`, `RoomID`, `FaultDescription`, `ReportedDate`, `ResolvedDate`, `TechnicianName`, `Cost`, `Status`) VALUES
(1, 2232, 'tuvalet servis', '2026-04-26 16:59:42', NULL, '', 4000.00, 'Pending'),
(2, 2224, 'dolab servisi', '2026-04-27 03:17:49', NULL, '', 6000.00, 'Pending'),
(3, 2244, 'yatak kirlimis', '2026-05-11 09:59:22', NULL, '', 4000.00, 'Pending');

-- --------------------------------------------------------

--
-- Table structure for table `manufacturers`
--

CREATE TABLE `manufacturers` (
  `ManufacturerID` int(11) NOT NULL,
  `Name` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `manufacturers`
--

INSERT INTO `manufacturers` (`ManufacturerID`, `Name`) VALUES
(6, 'Cappy'),
(15, 'Çaykur'),
(1, 'Coca-Cola'),
(13, 'Damla Su'),
(7, 'Dimes'),
(10, 'Erikli Su'),
(3, 'Fanta'),
(11, 'Hayat Su'),
(16, 'Nescafe'),
(2, 'Pepsi'),
(9, 'Pınar Meyve Suyu'),
(12, 'Saka Su'),
(4, 'Sprite'),
(8, 'Tropicana'),
(14, 'Türk Kahvesi'),
(5, 'Uludağ Gazoz');

-- --------------------------------------------------------

--
-- Table structure for table `market_stocks`
--

CREATE TABLE `market_stocks` (
  `MarketStockID` int(11) NOT NULL,
  `ProductID` int(11) NOT NULL,
  `StoreID` varchar(50) NOT NULL,
  `Quantity` int(11) DEFAULT 0,
  `Price` decimal(10,2) DEFAULT 0.00,
  `LastUpdated` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `market_stocks`
--

INSERT INTO `market_stocks` (`MarketStockID`, `ProductID`, `StoreID`, `Quantity`, `Price`, `LastUpdated`) VALUES
(4, 409080, 'MARKET_1', 2, 70.00, '2026-03-30 21:06:48'),
(5, 201090, 'MARKET_1', 1, 80.00, '2026-03-30 21:09:26'),
(6, 908062, 'LOKANTA', 46, 20.00, '2026-05-31 14:38:39'),
(7, 908063, 'LOKANTA', 83, 85.00, '2026-05-31 14:38:39'),
(8, 908060, 'LOKANTA', 14, 90.00, '2026-05-31 14:37:52'),
(9, 908065, 'LOKANTA', 72, 75.00, '2026-05-10 15:39:39'),
(10, 309080, 'LOKANTA', 18, 20.00, '2026-05-21 16:21:21'),
(11, 45673, 'LOKANTA', 152, 40.00, '2026-05-31 14:40:10'),
(12, 908064, 'LOKANTA', 51, 70.00, '2026-05-21 16:21:21'),
(13, 908061, 'LOKANTA', 15, 30.00, '2026-04-27 04:18:41');

-- --------------------------------------------------------

--
-- Table structure for table `payments`
--

CREATE TABLE `payments` (
  `PaymentID` int(11) NOT NULL,
  `ReservationID` int(11) DEFAULT NULL,
  `PaymentDate` datetime DEFAULT current_timestamp(),
  `Amount` decimal(10,2) NOT NULL,
  `PaymentMethod` varchar(50) DEFAULT NULL,
  `Status` varchar(20) DEFAULT NULL,
  `RoomAmount` decimal(10,2) DEFAULT 0.00,
  `LokantaAmount` decimal(10,2) DEFAULT 0.00,
  `TotalAmount` decimal(10,2) DEFAULT 0.00
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `payments`
--

INSERT INTO `payments` (`PaymentID`, `ReservationID`, `PaymentDate`, `Amount`, `PaymentMethod`, `Status`, `RoomAmount`, `LokantaAmount`, `TotalAmount`) VALUES
(1, 2, '2026-04-12 13:21:57', 0.00, 'Nakit', NULL, 1500.00, 80.00, 1580.00),
(2, 3, '2026-04-12 15:19:18', 0.00, 'Nakit', NULL, 1500.00, 0.00, 1500.00),
(3, 5, '2026-04-12 15:20:00', 0.00, 'Nakit', NULL, 1500.00, 0.00, 1500.00),
(4, 12, '2026-04-12 15:21:22', 0.00, 'Nakit', NULL, 800.00, 40.00, 840.00),
(5, 8, '2026-04-12 15:25:53', 0.00, 'Nakit', NULL, 800.00, 0.00, 800.00),
(6, 4, '2026-04-13 11:12:53', 0.00, 'Nakit', NULL, 1500.00, 80.00, 1580.00),
(7, 16, '2026-04-15 21:51:23', 0.00, 'Nakit', NULL, 0.00, 0.00, 66000.00),
(8, 17, '2026-04-16 09:13:33', 0.00, 'Nakit', NULL, 0.00, 0.00, 100000.00),
(9, 20, '2026-04-19 13:27:18', 0.00, 'Nakit/Kredi (Kısmi)', NULL, 0.00, 0.00, 500.00),
(10, 7, '2026-04-19 13:52:10', 0.00, 'Nakit/Kredi (Kısmi)', NULL, 0.00, 0.00, 400.00),
(11, 7, '2026-04-19 14:11:50', 0.00, '💳 Kredi Kartı (USD - Final)', NULL, 0.00, 0.00, 99999999.99),
(12, 20, '2026-04-20 00:05:51', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 500.00),
(13, 11, '2026-04-20 00:07:24', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 400.00),
(14, 10, '2026-04-20 00:17:08', 0.00, '💵 Nakit Ödeme (USD)', NULL, 0.00, 0.00, 17879.56),
(15, 11, '2026-04-20 09:30:19', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 250.00),
(16, 11, '2026-04-20 09:30:44', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 100.00),
(17, 11, '2026-04-20 09:31:38', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 60.00),
(18, 15, '2026-04-20 09:32:38', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 980.00),
(19, 11, '2026-04-20 09:37:19', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 110.00),
(20, 13, '2026-04-20 09:38:45', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 2000.00),
(21, 11, '2026-04-20 13:51:27', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 120.00),
(22, 11, '2026-04-20 13:51:38', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 0.00),
(23, 13, '2026-04-20 13:51:44', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 0.00),
(24, 14, '2026-04-20 13:51:54', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1500.00),
(25, 10, '2026-04-20 13:52:06', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 0.00),
(26, 16, '2026-04-20 13:52:21', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1200.00),
(27, 17, '2026-04-20 13:53:03', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1500.00),
(28, 19, '2026-04-20 13:53:12', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1500.00),
(29, 20, '2026-04-20 13:53:16', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 500.00),
(30, 21, '2026-04-20 13:55:04', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 620.00),
(31, 21, '2026-04-20 13:55:33', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 500.00),
(32, 18, '2026-04-24 18:27:20', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1500.00),
(33, 22, '2026-04-24 18:30:29', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1700.00),
(34, 23, '2026-04-24 19:42:49', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 10500.00),
(35, 26, '2026-04-24 19:42:54', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 800.00),
(36, 24, '2026-04-24 19:43:02', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 800.00),
(37, 25, '2026-04-24 19:43:08', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 800.00),
(38, 27, '2026-04-24 19:52:43', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 500.00),
(39, 27, '2026-04-24 19:53:01', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 200.00),
(40, 27, '2026-04-24 19:53:32', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 500.00),
(41, 29, '2026-04-24 20:03:14', 0.00, 'Girişte Nakit (TL)', NULL, 0.00, 0.00, 800.00),
(42, 29, '2026-04-26 16:07:44', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 960.00),
(43, 21, '2026-04-27 03:16:36', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 9580.00),
(44, 28, '2026-04-27 21:14:30', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 4620.00),
(45, 30, '2026-04-29 16:07:37', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 3290.00),
(46, 31, '2026-04-29 16:13:49', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 500.00),
(47, 27, '2026-04-29 16:14:27', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 2390.00),
(48, 27, '2026-04-29 16:15:25', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 1160.00),
(49, 27, '2026-04-29 16:16:00', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 0.00),
(50, 33, '2026-05-03 13:12:44', 0.00, '💳 Kredi Kartı (USD)', NULL, 0.00, 0.00, 41371.66),
(51, 33, '2026-05-03 13:13:01', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 0.00),
(52, 31, '2026-05-04 09:41:13', 0.00, '💵 Nakit Ödeme (TRY)', NULL, 0.00, 0.00, 410.00),
(53, 35, '2026-05-04 10:23:51', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 4800.00),
(54, 39, '2026-05-04 10:23:58', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 24800.00),
(55, 34, '2026-05-04 10:24:05', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 0.00),
(56, 32, '2026-05-04 10:24:14', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 4250.00),
(57, 31, '2026-05-04 10:24:25', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 7000.00),
(58, 45, '2026-05-11 09:26:02', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 155.00),
(59, 57, '2026-05-17 11:52:42', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 7500.00),
(60, 62, '2026-05-17 11:53:02', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1500.00),
(61, 49, '2026-05-17 11:53:22', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 150.00),
(62, 61, '2026-05-17 11:53:27', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 800.00),
(63, 63, '2026-05-17 11:53:36', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 800.00),
(64, 54, '2026-05-17 11:53:43', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 4800.00),
(65, 50, '2026-05-17 11:53:55', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 205.00),
(66, 58, '2026-05-17 11:54:04', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 4800.00),
(67, 56, '2026-05-17 11:54:13', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 155.00),
(68, 52, '2026-05-17 11:54:19', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 0.00),
(69, 64, '2026-05-17 11:54:36', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 800.00),
(70, 51, '2026-05-17 11:54:46', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 12215.00),
(71, 66, '2026-05-18 10:01:14', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1500.00),
(72, 67, '2026-05-21 16:17:38', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 4500.00),
(73, 73, '2026-05-21 16:17:56', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 2400.00),
(74, 72, '2026-05-21 16:19:06', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 2400.00),
(75, 68, '2026-05-21 16:19:14', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 2400.00),
(76, 76, '2026-05-21 16:41:16', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 800.00),
(77, 41, '2026-05-21 16:41:47', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 3250.00),
(78, 77, '2026-05-21 16:42:53', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1500.00),
(79, 78, '2026-05-22 00:00:47', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1000.00),
(80, 80, '2026-05-22 00:08:36', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 7500.00),
(81, 59, '2026-05-28 15:50:48', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 15090.00),
(82, 81, '2026-05-28 15:50:55', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 3200.00),
(83, 82, '2026-05-28 15:51:07', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 3200.00),
(84, 83, '2026-05-28 15:51:13', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 3200.00),
(85, 84, '2026-05-30 13:09:40', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 3215.00),
(86, 60, '2026-05-30 13:09:57', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 10400.00),
(87, 47, '2026-05-30 13:14:11', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 3200.00),
(88, 70, '2026-05-30 13:14:58', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 12000.00),
(89, 40, '2026-05-30 13:16:04', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 400.00),
(90, 42, '2026-05-30 13:16:46', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 24170.00),
(91, 38, '2026-05-30 13:18:02', 0.00, '💵 Nakit Ödeme (EUR - Final)', NULL, 0.00, 0.00, 0.00),
(92, 36, '2026-05-30 13:18:07', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 0.00),
(93, 69, '2026-05-30 13:21:28', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 7200.00),
(94, 86, '2026-05-31 10:55:27', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1000.00),
(95, 85, '2026-05-31 10:56:00', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1500.00),
(96, 88, '2026-05-31 10:56:08', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 0.00),
(97, 89, '2026-05-31 12:32:16', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 3000.00),
(98, 90, '2026-05-31 12:32:22', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 6000.00),
(99, 93, '2026-05-31 12:32:28', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1500.00),
(100, 91, '2026-05-31 12:33:37', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 6000.00),
(101, 92, '2026-05-31 12:33:43', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1500.00),
(102, 37, '2026-05-31 14:41:55', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 950.00),
(103, 97, '2026-06-02 10:36:24', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 800.00),
(104, 98, '2026-06-02 11:28:18', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 800.00),
(105, 94, '2026-06-02 11:28:26', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 1600.00),
(106, 96, '2026-06-02 11:29:43', 0.00, '💵 Nakit Ödeme (TRY - Final)', NULL, 0.00, 0.00, 3000.00);

-- --------------------------------------------------------

--
-- Table structure for table `products`
--

CREATE TABLE `products` (
  `ProductID` int(11) NOT NULL,
  `Barcode` varchar(50) NOT NULL,
  `ItemName` varchar(100) NOT NULL,
  `Category` varchar(50) DEFAULT NULL,
  `ManufacturerName` varchar(100) DEFAULT NULL,
  `Unit` varchar(20) DEFAULT NULL,
  `Price` decimal(10,2) DEFAULT 0.00,
  `SuggestedSalePrice` decimal(10,2) DEFAULT NULL,
  `CreatedAt` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `products`
--

INSERT INTO `products` (`ProductID`, `Barcode`, `ItemName`, `Category`, `ManufacturerName`, `Unit`, `Price`, `SuggestedSalePrice`, `CreatedAt`) VALUES
(45673, 'PRD-45673', 'cay', '', 'lıptn', 'Adet', 40.00, 40.00, '2026-04-06 10:05:34'),
(201090, 'PRD-201090', 'fanta', '', '', '', 70.00, 70.00, '2026-03-30 21:08:22'),
(309080, 'PRD-309080', 'su', '', 'hayatfabrik', '', 20.00, 20.00, '2026-03-30 21:01:31'),
(409080, 'PRD-409080', 'kole', NULL, '', NULL, 80.00, NULL, '2026-03-30 21:05:23'),
(908060, 'PRD-908060', 'çorba', '', 'bim', 'Adet', 50.00, 50.00, '2026-03-30 21:15:04'),
(908061, '106879', 'siyah çay', '', 'Çay', '', 30.00, 30.00, '2026-04-12 12:17:12'),
(908062, 'siyah çay', '106879', '', 'Çay', '', 20.00, 20.00, '2026-04-12 12:23:46'),
(908063, '100356', 'coca-cola 1lt', '', 'Coca-Cola', '', 85.00, 85.00, '2026-04-13 10:49:55'),
(908064, '120046', 'fanta', '', 'Fanta', '', 70.00, 70.00, '2026-04-13 11:01:03'),
(908065, '10067', 'pepsı 1lt', '', 'Pepsi', '', 75.00, 75.00, '2026-04-13 11:05:40');

-- --------------------------------------------------------

--
-- Table structure for table `reservations`
--

CREATE TABLE `reservations` (
  `ReservationID` int(11) NOT NULL,
  `CustomerID` int(11) DEFAULT NULL,
  `RoomID` int(11) DEFAULT NULL,
  `BedNumber` int(11) DEFAULT 1,
  `CheckInDate` date NOT NULL,
  `CheckOutDate` date NOT NULL,
  `Status` varchar(20) DEFAULT 'CheckedIn',
  `TotalAmount` decimal(10,2) DEFAULT NULL,
  `PaidAmount` decimal(10,2) DEFAULT 0.00,
  `CreatedAt` datetime DEFAULT current_timestamp(),
  `Agency` varchar(50) DEFAULT NULL,
  `StayType` varchar(50) DEFAULT NULL,
  `Notes` text DEFAULT NULL,
  `Color` varchar(10) DEFAULT NULL,
  `Currency` varchar(10) DEFAULT 'TL',
  `ChannelName` varchar(50) DEFAULT 'Direkt',
  `CommissionAmount` decimal(10,2) DEFAULT 0.00,
  `CompanyID` int(11) DEFAULT NULL,
  `ExtraAmount` decimal(10,2) DEFAULT 0.00,
  `IsOnline` tinyint(1) DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `reservations`
--

INSERT INTO `reservations` (`ReservationID`, `CustomerID`, `RoomID`, `BedNumber`, `CheckInDate`, `CheckOutDate`, `Status`, `TotalAmount`, `PaidAmount`, `CreatedAt`, `Agency`, `StayType`, `Notes`, `Color`, `Currency`, `ChannelName`, `CommissionAmount`, `CompanyID`, `ExtraAmount`, `IsOnline`) VALUES
(1, 1, 1, 1, '2026-03-23', '2026-03-26', 'CheckedOut', 1500.00, 0.00, '2026-03-23 09:37:31', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(2, 8, 1, 1, '2026-03-27', '2026-03-28', 'CheckedOut', 1500.00, 0.00, '2026-03-27 11:50:08', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(3, 2, 2, 1, '2026-03-27', '2026-03-28', 'CheckedOut', 1500.00, 0.00, '2026-03-27 11:58:35', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(4, 4, 2, 2, '2026-03-27', '2026-03-28', 'CheckedOut', 1500.00, 0.00, '2026-03-27 11:59:06', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(5, 11, 2, 1, '2026-03-30', '2026-03-31', 'CheckedOut', 1500.00, 0.00, '2026-03-30 09:31:52', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(6, 13, 3, 1, '2026-03-30', '2026-03-31', 'CheckedOut', 800.00, 0.00, '2026-03-30 11:06:16', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(7, 15, 4, 4, '2026-04-05', '2026-04-06', 'CheckedOut', 800.00, 99999999.99, '2026-04-05 13:22:18', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(8, 16, 4, 1, '2026-04-05', '2026-04-06', 'CheckedOut', 800.00, 0.00, '2026-04-05 13:33:36', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(9, 17, 5, 2, '2026-04-06', '2026-04-07', 'CheckedOut', 800.00, 0.00, '2026-04-06 09:30:58', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(10, 17, 5, 1, '2026-04-06', '2026-04-07', 'CheckedOut', 800.00, 17879.56, '2026-04-06 10:17:03', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(11, 17, 6, 1, '2026-04-06', '2026-04-07', 'CheckedOut', 800.00, 1040.00, '2026-04-06 10:17:23', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(12, 17, 6, 2, '2026-04-06', '2026-04-07', 'CheckedOut', 800.00, 0.00, '2026-04-06 10:24:50', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(13, 17, 7, 1, '2026-04-06', '2026-04-07', 'CheckedOut', 1500.00, 2000.00, '2026-04-06 11:48:20', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(14, 17, 7, 2, '2026-04-06', '2026-04-07', 'CheckedOut', 1500.00, 1500.00, '2026-04-06 11:48:20', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(15, 18, 3, 1, '2026-04-13', '2026-04-14', 'CheckedOut', 800.00, 980.00, '2026-04-13 09:50:56', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(16, 3, 9, 3, '2026-04-13', '2026-04-14', 'CheckedOut', 800.00, 1200.00, '2026-04-13 11:04:42', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(17, 20, 1, 1, '2026-04-16', '2026-04-17', 'CheckedOut', 1500.00, 1500.00, '2026-04-16 09:13:05', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(18, 17, 13, 1, '2026-04-16', '2026-04-17', 'CheckedOut', 1500.00, 1500.00, '2026-04-16 12:41:44', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(19, 17, 14, 1, '2026-04-16', '2026-04-17', 'CheckedOut', 1500.00, 1500.00, '2026-04-16 12:42:22', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(20, 21, 2, 1, '2026-04-19', '2026-04-19', 'CheckedOut', 1500.00, 1500.00, '2026-04-19 13:26:41', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(21, 20, 1, 1, '2026-04-20', '2026-04-27', 'CheckedOut', 10500.00, 10700.00, '2026-04-20 13:53:58', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(22, 19, 2, 1, '2026-04-20', '2026-04-21', 'CheckedOut', 1500.00, 1700.00, '2026-04-20 13:54:18', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(23, 19, 2, 1, '2026-04-18', '2026-04-25', 'CheckedOut', 10500.00, 10500.00, '2026-04-24 18:48:29', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(24, 22, 3, 1, '2026-04-24', '2026-04-25', 'CheckedOut', 800.00, 800.00, '2026-04-24 19:10:38', 'Secıniz', 'Sadece oda', '', '#4F46E5', 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(25, 3, 4, 1, '2026-04-24', '2026-04-25', 'CheckedOut', 800.00, 800.00, '2026-04-24 19:16:53', 'Secıniz', 'Sadece oda', '', '#4F46E5', 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(26, 20, 5, 1, '2026-04-24', '2026-04-25', 'CheckedOut', 800.00, 800.00, '2026-04-24 19:31:18', 'Direkt', 'Sadece Oda', '', '#4F46E5', 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(27, 18, 4, 1, '2026-04-24', '2026-04-29', 'CheckedOut', 4000.00, 4750.00, '2026-04-24 19:51:44', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(28, 21, 2, 1, '2026-04-24', '2026-04-27', 'CheckedOut', 4500.00, 4620.00, '2026-04-24 19:55:27', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(29, 17, 2221, 1, '2026-04-24', '2026-04-26', 'CheckedOut', 1600.00, 1760.00, '2026-04-24 20:03:14', NULL, NULL, NULL, NULL, 'TL', 'Direkt', 0.00, NULL, 0.00, 0),
(30, 18, 1, 2, '2026-04-27', '2026-04-29', 'CheckedOut', 3000.00, 3290.00, '2026-04-27 21:07:42', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(31, 11, 2, 1, '2026-04-29', '2026-05-04', 'CheckedOut', 7500.00, 7910.00, '2026-04-29 16:10:40', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(32, 22, 2241, 1, '2026-04-29', '2026-05-04', 'CheckedOut', 4000.00, 4250.00, '2026-04-29 16:12:05', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(33, 14, 3, 1, '2026-04-29', '2026-05-03', 'CheckedOut', 4800.00, 41371.66, '2026-04-29 22:03:11', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(34, 21, 1, 1, '2026-05-04', '2026-05-05', 'CheckedOut', 0.00, 0.00, '2026-05-04 09:48:27', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(35, 18, 3, 1, '2026-06-01', '2026-06-05', 'CheckedOut', 4800.00, 4800.00, '2026-05-04 09:51:23', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(36, 4, 4, 1, '2026-06-02', '2026-06-03', 'CheckedOut', 0.00, 0.00, '2026-05-04 10:00:35', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(37, 4, 11, 1, '2026-06-02', '2026-06-03', 'CheckedOut', 800.00, 950.00, '2026-05-04 10:01:00', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(38, 14, 2223, 1, '2026-06-02', '2026-06-03', 'CheckedOut', 0.00, 0.00, '2026-05-04 10:01:47', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(39, 2, 2223, 2, '2026-05-04', '2026-06-04', 'CheckedOut', 24800.00, 24800.00, '2026-05-04 10:02:31', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(40, 22, 1, 1, '2026-06-01', '2026-06-05', 'CheckedOut', 400.00, 400.00, '2026-05-04 10:25:11', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(41, 2, 1, 2, '2026-05-04', '2026-06-02', 'CheckedOut', 2900.00, 3250.00, '2026-05-04 10:25:43', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(42, 21, 4, 2, '2026-05-04', '2026-06-03', 'CheckedOut', 24000.00, 24170.00, '2026-05-04 10:26:54', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(43, 17, 2223, 2, '2026-05-04', '2026-06-04', 'CheckedIn', 24800.00, 0.00, '2026-05-04 10:35:43', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(44, 20, 2, 1, '2026-05-05', '2026-05-05', 'NoShow', 0.00, 0.00, '2026-05-04 10:50:55', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(45, 13, 2, 2, '2026-05-04', '2026-05-11', 'CheckedOut', 0.00, 155.00, '2026-05-04 10:51:37', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(46, 18, 3, 1, '2026-05-05', '2026-05-05', 'NoShow', 0.00, 0.00, '2026-05-04 10:52:23', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(47, 18, 2221, 1, '2026-06-01', '2026-06-05', 'CheckedOut', 3200.00, 3200.00, '2026-05-04 10:53:07', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(48, 20, 2221, 2, '2026-05-05', '2026-06-04', 'NoShow', 24000.00, 0.00, '2026-05-04 10:59:24', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(49, 15, 3, 1, '2026-05-04', '2026-05-17', 'CheckedOut', 0.00, 150.00, '2026-05-04 23:20:23', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(50, 22, 9, 1, '2026-05-09', '2026-05-17', 'CheckedOut', 0.00, 205.00, '2026-05-09 16:03:49', NULL, NULL, NULL, NULL, 'TL', 'Direct', 60.00, NULL, 0.00, 0),
(51, 4, 12, 1, '2026-05-09', '2026-05-17', 'CheckedOut', 12000.00, 12215.00, '2026-05-09 16:05:48', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(52, 16, 13, 1, '2026-05-10', '2026-05-17', 'CheckedOut', 0.00, 0.00, '2026-05-10 11:12:39', NULL, NULL, NULL, NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(53, 13, 2232, 1, '2026-05-12', '2026-05-18', 'NoShow', 4800.00, 0.00, '2026-05-11 09:47:39', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(54, 19, 2221, 1, '2026-05-11', '2026-05-17', 'CheckedOut', 4800.00, 4800.00, '2026-05-11 10:02:44', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(55, 24, 14, 2, '2026-05-12', '2026-05-13', 'NoShow', 1000.00, 0.00, '2026-05-11 10:51:23', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(56, 24, 14, 1, '2026-05-11', '2026-05-17', 'CheckedOut', 0.00, 155.00, '2026-05-11 10:52:13', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(57, 25, 1, 1, '2026-05-12', '2026-05-17', 'CheckedOut', 7500.00, 7500.00, '2026-05-11 19:35:08', NULL, NULL, 'Deneme Rezervasyonu', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(58, 26, 2243, 1, '2026-05-11', '2026-05-17', 'CheckedOut', 4800.00, 4800.00, '2026-05-11 20:01:18', NULL, NULL, '', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(59, 27, 7, 1, '2026-05-18', '2026-05-28', 'CheckedOut', 15000.00, 15090.00, '2026-05-11 20:05:25', NULL, NULL, '', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(60, 29, 10, 4, '2026-05-17', '2026-05-30', 'CheckedOut', 10400.00, 10400.00, '2026-05-11 20:19:24', NULL, NULL, '', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(61, 30, 5, 1, '2026-05-16', '2026-05-17', 'CheckedOut', 800.00, 800.00, '2026-05-16 16:06:48', NULL, NULL, 'waa socodsaday', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(62, 31, 2, 2, '2026-05-16', '2026-05-17', 'CheckedOut', 1500.00, 1500.00, '2026-05-16 16:32:14', NULL, NULL, '', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(63, 16, 6, 2, '2026-05-16', '2026-05-17', 'CheckedOut', 800.00, 800.00, '2026-05-16 16:43:29', NULL, NULL, 'Online Rezervasyon (RSV-C1FB2308)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(64, 32, 2234, 4, '2026-05-16', '2026-05-17', 'CheckedOut', 800.00, 800.00, '2026-05-16 16:55:11', NULL, NULL, 'Online Rezervasyon (RSV-83488ED3)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(65, 25, 1, 1, '2026-05-18', '2026-05-20', 'NoShow', 3000.00, 0.00, '2026-05-17 11:40:15', NULL, NULL, 'Online Rezervasyon (RSV-79946234)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(66, 33, 8, 1, '2026-05-17', '2026-05-18', 'CheckedOut', 1500.00, 1500.00, '2026-05-17 11:46:33', NULL, NULL, 'Online Rezervasyon (RSV-E77C472D)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(67, 34, 2, 1, '2026-05-18', '2026-05-21', 'CheckedOut', 4500.00, 4500.00, '2026-05-18 09:05:45', NULL, NULL, 'Online Rezervasyon (RSV-8AE55D4E)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(68, 20, 2241, 1, '2026-05-18', '2026-05-21', 'CheckedOut', 2400.00, 2400.00, '2026-05-18 09:19:30', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(69, 31, 2234, 1, '2026-05-23', '2026-06-01', 'CheckedOut', 7200.00, 7200.00, '2026-05-18 09:28:54', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(70, 18, 7, 2, '2026-05-24', '2026-06-01', 'CheckedOut', 12000.00, 12000.00, '2026-05-18 09:30:01', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(71, 34, 3, 1, '2026-05-20', '2026-05-29', 'NoShow', 7200.00, 0.00, '2026-05-18 09:30:57', NULL, NULL, 'Online Rezervasyon (RSV-CD3356B7)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(72, 35, 2222, 1, '2026-05-18', '2026-05-21', 'CheckedOut', 2400.00, 2400.00, '2026-05-18 09:33:18', NULL, NULL, 'Online Rezervasyon (RSV-E1B4486A)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(73, 36, 4, 1, '2026-05-18', '2026-05-21', 'CheckedOut', 2400.00, 2400.00, '2026-05-18 09:58:47', NULL, NULL, 'Online Rezervasyon (RSV-9540FC47)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(74, 37, 2, 2, '2026-05-18', '2026-05-19', 'NoShow', 1500.00, 0.00, '2026-05-18 10:35:19', NULL, NULL, 'Online Rezervasyon (RSV-3AC18DBC)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(75, 16, 4, 3, '2026-05-18', '2026-05-19', 'NoShow', 800.00, 0.00, '2026-05-18 10:49:36', NULL, NULL, 'Online Rezervasyon (RSV-DB5C33BE)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(76, 38, 5, 1, '2026-05-21', '2026-05-22', 'CheckedOut', 800.00, 800.00, '2026-05-21 16:23:27', NULL, NULL, 'Online Rezervasyon (RSV-F85DD61E)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(77, 39, 1, 1, '2026-05-21', '2026-05-22', 'CheckedOut', 1500.00, 1500.00, '2026-05-21 16:28:11', NULL, NULL, 'Online Rezervasyon (RSV-D068D670)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(78, 38, 1, 1, '2026-05-21', '2026-05-22', 'CheckedOut', 1000.00, 1000.00, '2026-05-21 22:42:02', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(79, 40, 2222, 1, '2026-05-21', '2026-05-22', 'NoShow', 800.00, 0.00, '2026-05-21 23:59:34', NULL, NULL, 'Online Rezervasyon (RSV-42BADBD5)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(80, 44, 1, 1, '2026-05-23', '2026-05-28', 'CheckedOut', 7500.00, 7500.00, '2026-05-22 00:07:38', NULL, NULL, 'Online Rezervasyon (RSV-65DE877C)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(81, 16, 12, 1, '2026-05-24', '2026-05-28', 'CheckedOut', 3200.00, 3200.00, '2026-05-24 12:25:14', NULL, NULL, 'Online Rezervasyon (RSV-7495FD3F)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(82, 16, 2241, 1, '2026-05-24', '2026-05-28', 'CheckedOut', 3200.00, 3200.00, '2026-05-24 13:01:25', NULL, NULL, 'Online Rezervasyon (RSV-753161FB)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(83, 47, 2243, 3, '2026-05-24', '2026-05-28', 'CheckedOut', 3200.00, 3200.00, '2026-05-24 13:03:48', NULL, NULL, 'Online Rezervasyon (RSV-58AF458B)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(84, 48, 8, 1, '2026-05-28', '2026-05-30', 'CheckedOut', 3000.00, 3215.00, '2026-05-28 13:18:46', NULL, NULL, 'Online Rezervasyon (RSV-BA799C45)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(85, 49, 13, 1, '2026-05-30', '2026-05-31', 'CheckedOut', 1500.00, 1500.00, '2026-05-30 11:54:45', NULL, NULL, 'Online Rezervasyon (RSV-D37E4A48)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(86, 23, 1, 1, '2026-05-30', '2026-05-31', 'CheckedOut', 1000.00, 1000.00, '2026-05-30 11:58:47', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(87, 48, 17, 1, '2026-06-04', '2026-06-08', 'Reserved', 3200.00, 0.00, '2026-05-30 13:06:22', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(88, 16, 17, 1, '2026-05-30', '2026-05-31', 'CheckedOut', 0.00, 0.00, '2026-05-30 13:07:48', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(89, 25, 2, 1, '2026-05-31', '2026-06-02', 'CheckedOut', 3000.00, 3000.00, '2026-05-30 13:22:53', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(90, 25, 1, 1, '2026-06-01', '2026-06-05', 'CheckedOut', 6000.00, 6000.00, '2026-05-31 11:44:04', NULL, NULL, 'Test (RSV-DAA64E35)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(91, 53, 1, 2, '2026-07-01', '2026-07-05', 'CheckedOut', 6000.00, 6000.00, '2026-05-31 11:48:31', NULL, NULL, 'Test3 (RSV-03C29427)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(92, 25, 1, 1, '2026-05-31', '2026-06-01', 'CheckedOut', 1500.00, 1500.00, '2026-05-31 11:58:12', NULL, NULL, 'Test rez (RSV-79FCFC34)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(93, 25, 1, 2, '2026-05-31', '2026-06-01', 'CheckedOut', 1500.00, 1500.00, '2026-05-31 12:00:34', NULL, NULL, 'Test rez (RSV-172A520D)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(94, 49, 10, 1, '2026-05-31', '2026-06-02', 'CheckedOut', 1600.00, 1600.00, '2026-05-31 12:29:59', NULL, NULL, 'Online Rezervasyon (RSV-5A31ED44)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(95, 54, 12, 3, '2026-05-31', '2026-06-01', 'CheckedIn', 800.00, 0.00, '2026-05-31 12:34:39', NULL, NULL, 'Online Rezervasyon (RSV-8A80C533)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(96, 34, 13, 1, '2026-05-31', '2026-06-02', 'CheckedOut', 3000.00, 3000.00, '2026-05-31 14:43:45', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(97, 20, 5, 1, '2026-06-01', '2026-06-02', 'CheckedOut', 800.00, 800.00, '2026-06-01 09:21:39', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(98, 55, 10, 1, '2026-06-01', '2026-06-02', 'CheckedOut', 800.00, 800.00, '2026-06-01 09:24:30', NULL, NULL, 'Online Rezervasyon (RSV-AB2A1750)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(99, 23, 10, 1, '2026-06-03', '2026-06-04', 'NoShow', 800.00, 0.00, '2026-06-01 09:29:30', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(100, 56, 8, 1, '2026-06-12', '2026-06-19', 'Reserved', 10500.00, 0.00, '2026-06-01 10:01:17', NULL, NULL, 'Temiz olsun. Bana özel garson istiyorum. Ücretsiz 1 haftalık kiralık üstü açık porsche istiyorum. (RSV-780FA239)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(101, 57, 2, 1, '2026-06-02', '2026-06-03', 'CheckedIn', 1500.00, 0.00, '2026-06-02 10:06:54', NULL, NULL, 'Online Rezervasyon (RSV-9DA997E7)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(102, 3, 3, 1, '2026-06-02', '2026-06-03', 'CheckedIn', 0.00, 0.00, '2026-06-02 10:38:17', NULL, NULL, '', NULL, 'TL', 'Direct', 0.00, NULL, 0.00, 0),
(103, 58, 13, 1, '2026-06-02', '2026-06-03', 'CheckedIn', 1500.00, 0.00, '2026-06-02 10:41:46', NULL, NULL, 'Online Rezervasyon (RSV-E86E5A54)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(104, 59, 2243, 1, '2026-06-02', '2026-06-03', 'CheckedIn', 800.00, 0.00, '2026-06-02 11:21:32', NULL, NULL, 'Online Rezervasyon (RSV-05CCB5A4)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1),
(105, 61, 2, 2, '2026-06-02', '2026-06-03', 'CheckedIn', 1500.00, 0.00, '2026-06-02 11:31:00', NULL, NULL, 'Online Rezervasyon (RSV-57FEB77F)', NULL, 'TL', 'Web API', 0.00, NULL, 0.00, 1);

-- --------------------------------------------------------

--
-- Table structure for table `restaurant_tables`
--

CREATE TABLE `restaurant_tables` (
  `TableID` int(11) NOT NULL,
  `TableName` varchar(50) NOT NULL,
  `Status` varchar(20) DEFAULT 'Available',
  `CurrentReservationID` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `restaurant_tables`
--

INSERT INTO `restaurant_tables` (`TableID`, `TableName`, `Status`, `CurrentReservationID`) VALUES
(1, 'Masa 1', 'Occupied', NULL),
(2, 'Masa 2', 'Available', NULL),
(3, 'Masa 3', 'Available', NULL),
(4, 'Masa 4', 'Available', NULL),
(5, 'Masa 5', 'Available', NULL),
(6, 'Bahçe 1', 'Available', NULL),
(7, 'Bahçe 2', 'Available', NULL),
(8, 'Teras 1', 'Available', NULL),
(9, 'Teras 2', 'Available', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `rooms`
--

CREATE TABLE `rooms` (
  `RoomID` int(11) NOT NULL,
  `RoomNumber` varchar(10) NOT NULL,
  `FloorID` int(11) DEFAULT NULL,
  `RoomTypeID` int(11) DEFAULT NULL,
  `Capacity` int(11) DEFAULT 2,
  `OccupiedBeds` int(11) DEFAULT 0,
  `Status` varchar(20) DEFAULT 'Available',
  `Description` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `rooms`
--

INSERT INTO `rooms` (`RoomID`, `RoomNumber`, `FloorID`, `RoomTypeID`, `Capacity`, `OccupiedBeds`, `Status`, `Description`) VALUES
(1, '101', 1, 1, 2, 0, 'Available', NULL),
(2, '102', 1, 1, 3, 2, 'Partial', NULL),
(3, '103', 1, 2, 1, 1, 'Occupied', NULL),
(4, '104', 1, 2, 4, 0, 'Available', NULL),
(5, '105', 1, 2, 2, 0, 'Available', NULL),
(6, '106', 1, 2, 3, 0, 'Available', NULL),
(7, '201', 2, 1, 2, 0, 'Available', NULL),
(8, '202', 2, 1, 1, 0, 'Available', NULL),
(9, '203', 2, 2, 3, 0, 'Available', NULL),
(10, '204', 2, 2, 4, 0, 'Available', NULL),
(11, '205', 2, 2, 2, 0, 'Available', NULL),
(12, '206', 2, 2, 3, 1, 'Partial', NULL),
(13, '301', 3, 1, 1, 1, 'Occupied', NULL),
(14, '302', 3, 1, 2, 0, 'Available', NULL),
(15, '303', 3, 2, 3, 0, 'Available', NULL),
(16, '304', 3, 2, 4, 0, 'Available', NULL),
(17, '305', 3, 2, 2, 0, 'Available', NULL),
(18, '306', 3, 2, 1, 0, 'Available', NULL),
(2221, '107', 1, 2, 2, 0, 'Available', NULL),
(2222, '108', 1, 2, 4, 0, 'Available', NULL),
(2223, '109', 1, 2, 3, 1, 'Partial', NULL),
(2224, '110', 1, 2, 4, 0, 'Available', NULL),
(2231, '207', 2, 2, 2, 0, 'Available', NULL),
(2232, '208', 2, 2, 4, 0, 'Available', NULL),
(2233, '209', 2, 2, 3, 0, 'Available', NULL),
(2234, '210', 2, 2, 4, 0, 'Available', NULL),
(2241, '307', 3, 2, 2, 0, 'Available', NULL),
(2242, '308', 3, 2, 4, 0, 'Available', NULL),
(2243, '309', 3, 2, 3, 1, 'Partial', NULL),
(2244, '310', 3, 2, 4, 0, 'Maintenance', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `room_prices`
--

CREATE TABLE `room_prices` (
  `PriceID` int(11) NOT NULL,
  `RoomTypeID` int(11) DEFAULT NULL,
  `RoomID` int(11) DEFAULT NULL,
  `RoomNumber` varchar(50) DEFAULT NULL,
  `StartDate` date DEFAULT NULL,
  `EndDate` date DEFAULT NULL,
  `Price` decimal(10,2) DEFAULT NULL,
  `DayOfWeek` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `room_prices`
--

INSERT INTO `room_prices` (`PriceID`, `RoomTypeID`, `RoomID`, `RoomNumber`, `StartDate`, `EndDate`, `Price`, `DayOfWeek`) VALUES
(1, 1, NULL, NULL, '2026-03-23', NULL, 1000.00, NULL),
(2, 1, NULL, NULL, '2026-03-23', NULL, 800.00, NULL),
(3, 1, NULL, NULL, '2026-03-23', NULL, 700.00, NULL),
(4, NULL, 1, '101', '2026-03-23', NULL, 1000.00, NULL),
(5, NULL, 6, '106', '2026-03-27', NULL, 100.00, NULL),
(6, NULL, 6, '106', '2026-03-27', NULL, 1000.00, NULL),
(7, NULL, 3, '103', '2026-03-27', NULL, 1000.00, NULL),
(8, NULL, 3, '103', '2026-03-27', NULL, 1100.00, NULL),
(9, NULL, 3, '103', '2026-03-27', NULL, 1200.00, NULL),
(10, NULL, 1, '101', '2026-03-30', NULL, 100.00, NULL),
(11, NULL, 14, '302', '2026-04-04', NULL, 1000.00, NULL),
(12, NULL, 1, '101', '2026-05-30', NULL, 1000.00, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `room_types`
--

CREATE TABLE `room_types` (
  `RoomTypeID` int(11) NOT NULL,
  `TypeName` varchar(50) NOT NULL,
  `Description` text DEFAULT NULL,
  `MaxOccupancy` int(11) DEFAULT 2,
  `BasePrice` decimal(10,2) DEFAULT 0.00
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `room_types`
--

INSERT INTO `room_types` (`RoomTypeID`, `TypeName`, `Description`, `MaxOccupancy`, `BasePrice`) VALUES
(1, 'Deniz Manzarali', 'Deniz manzarali oda', 4, 1500.00),
(2, 'Standart', 'Standart oda', 4, 800.00);

-- --------------------------------------------------------

--
-- Table structure for table `sales_log`
--

CREATE TABLE `sales_log` (
  `SaleID` int(11) NOT NULL,
  `ProductID` int(11) NOT NULL,
  `StoreID` varchar(50) NOT NULL,
  `Quantity` int(11) NOT NULL,
  `UnitPrice` decimal(10,2) NOT NULL,
  `TotalPrice` decimal(10,2) NOT NULL,
  `RoomInfo` varchar(255) DEFAULT '',
  `IsPaid` tinyint(4) DEFAULT 0,
  `Status` varchar(20) DEFAULT 'Pending',
  `SaleDate` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `sales_log`
--

INSERT INTO `sales_log` (`SaleID`, `ProductID`, `StoreID`, `Quantity`, `UnitPrice`, `TotalPrice`, `RoomInfo`, `IsPaid`, `Status`, `SaleDate`) VALUES
(1, 908062, 'LOKANTA', 2, 40.00, 80.00, 'Oda 101 - van hat', 0, 'Served', '2026-04-12 13:21:13'),
(2, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 106 - asıya malı', 1, 'Served', '2026-04-12 15:20:38'),
(3, 908063, 'LOKANTA', 1, 80.00, 80.00, 'Oda 102 - muat sari', 0, 'Served', '2026-04-13 11:11:24'),
(4, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 105 - asıya malı', 1, 'Served', '2026-04-15 21:37:20'),
(5, 908063, 'LOKANTA', 2, 80.00, 160.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-04-15 21:38:36'),
(6, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 201 - asıya malı', 1, 'Served', '2026-04-15 21:58:44'),
(7, 908060, 'LOKANTA', 2, 90.00, 180.00, 'Oda 103 - yahye muhan', 1, 'Served', '2026-04-15 21:59:07'),
(8, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 104 - salad dahır', 1, 'Served', '2026-04-19 14:00:52'),
(9, 908063, 'LOKANTA', 1, 80.00, 80.00, 'Oda 106 - asıya malı', 1, 'Served', '2026-04-20 09:24:49'),
(10, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-04-20 10:23:21'),
(11, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 106 - asıya malı', 1, 'Served', '2026-04-20 10:47:34'),
(12, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-04-20 13:17:58'),
(13, 908063, 'LOKANTA', 1, 80.00, 80.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-04-20 13:17:58'),
(14, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-04-20 13:17:58'),
(15, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-04-20 13:27:58'),
(16, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-04-20 13:27:58'),
(17, 908063, 'LOKANTA', 1, 80.00, 80.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-04-20 13:27:58'),
(18, 908063, 'LOKANTA', 1, 80.00, 80.00, 'Oda 106 - asıya malı', 1, 'Served', '2026-04-20 13:50:39'),
(19, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 106 - asıya malı', 1, 'Served', '2026-04-20 13:50:39'),
(20, 908063, 'LOKANTA', 1, 80.00, 80.00, 'Oda 101 - farobadan mohan', 1, 'Served', '2026-04-20 13:54:34'),
(21, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 101 - farobadan mohan', 1, 'Served', '2026-04-20 13:54:34'),
(22, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 101 - farobadan mohan', 1, 'Served', '2026-04-20 13:54:34'),
(23, 908063, 'LOKANTA', 2, 80.00, 160.00, 'Oda 102 - ahed yare muhan', 1, 'Served', '2026-04-21 10:55:25'),
(24, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 102 - ahed yare muhan', 1, 'Served', '2026-04-21 10:55:25'),
(25, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 102 - ahed yare muhan', 1, 'Served', '2026-04-21 10:55:25'),
(26, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-24 19:52:15'),
(27, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-24 19:52:15'),
(28, 908064, 'LOKANTA', 1, 70.00, 70.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-24 19:52:15'),
(29, 908063, 'LOKANTA', 1, 80.00, 80.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-24 19:53:46'),
(30, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-24 19:53:46'),
(31, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-24 19:53:46'),
(32, 908064, 'LOKANTA', 1, 70.00, 70.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-24 19:53:46'),
(33, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-24 19:53:46'),
(34, 908063, 'LOKANTA', 1, 80.00, 80.00, 'Oda 107 - asıya malı', 1, 'Served', '2026-04-24 20:03:40'),
(35, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 107 - asıya malı', 1, 'Served', '2026-04-24 20:03:40'),
(36, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 107 - asıya malı', 1, 'Served', '2026-04-24 20:03:40'),
(37, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 101 - farobadan mohan', 1, 'Served', '2026-04-26 16:05:50'),
(38, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 101 - farobadan mohan', 1, 'Served', '2026-04-26 16:05:50'),
(39, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-27 03:30:12'),
(40, 908064, 'LOKANTA', 1, 70.00, 70.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-27 03:30:12'),
(41, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 102 - muhubo jamac ısmaıl', 1, 'Served', '2026-04-27 04:19:35'),
(42, 908062, 'LOKANTA', 2, 40.00, 80.00, 'Oda 102 - muhubo jamac ısmaıl', 1, 'Served', '2026-04-27 04:19:35'),
(43, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 101 - yahye muhan', 1, 'Served', '2026-04-27 21:15:39'),
(44, 908064, 'LOKANTA', 1, 70.00, 70.00, 'Oda 101 - yahye muhan', 1, 'Served', '2026-04-27 21:15:40'),
(45, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 101 - yahye muhan', 1, 'Served', '2026-04-27 21:15:40'),
(46, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 101 - yahye muhan', 1, 'Served', '2026-04-27 21:15:53'),
(47, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-29 16:15:01'),
(48, 908064, 'LOKANTA', 1, 70.00, 70.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-29 16:15:01'),
(49, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 104 - yahye muhan', 1, 'Served', '2026-04-29 16:15:01'),
(50, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 307 - ahmett yare farah', 1, 'Served', '2026-04-29 22:04:12'),
(51, 908064, 'LOKANTA', 1, 70.00, 70.00, 'Oda 307 - ahmett yare farah', 1, 'Served', '2026-04-29 22:04:12'),
(52, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 102 - efe sam', 1, 'Served', '2026-04-30 10:44:55'),
(53, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 102 - efe sam', 1, 'Served', '2026-04-30 10:44:55'),
(54, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 103 - salad dahır', 1, 'Served', '2026-04-30 10:57:24'),
(55, 908063, 'LOKANTA', 1, 80.00, 80.00, 'Oda 103 - salad dahır', 1, 'Served', '2026-04-30 10:57:24'),
(56, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 102 - efe sam', 1, 'Served', '2026-05-03 22:52:54'),
(57, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 102 - efe sam', 1, 'Served', '2026-05-03 22:52:54'),
(58, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 307 - ahmett yare farah', 1, 'Served', '2026-05-03 23:02:55'),
(59, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Masa 3 | Oda 102 - efe sam', 0, 'Served', '2026-05-04 09:23:39'),
(60, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Masa 3 | Oda 102 - efe sam', 0, 'Served', '2026-05-04 09:23:39'),
(61, 908063, 'LOKANTA', 2, 80.00, 160.00, 'Oda 102 - efe sam', 1, 'Served', '2026-05-04 09:40:18'),
(62, 908062, 'LOKANTA', 1, 40.00, 40.00, 'Oda 102 - efe sam', 1, 'Served', '2026-05-04 09:40:18'),
(63, 908062, 'LOKANTA', 2, 30.00, 60.00, 'Oda 109 - asıya malı', 0, 'Served', '2026-05-04 23:18:12'),
(64, 908063, 'LOKANTA', 2, 85.00, 170.00, 'Oda 104 - muhubo jamac ısmaıl', 1, 'Served', '2026-05-04 23:19:00'),
(65, 908064, 'LOKANTA', 1, 70.00, 70.00, 'Oda 102 - kamal gam', 1, 'Served', '2026-05-10 14:58:08'),
(66, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 102 - kamal gam', 1, 'Served', '2026-05-10 14:58:08'),
(67, 908063, 'LOKANTA', 1, 85.00, 85.00, 'Oda 102 - kamal gam', 1, 'Served', '2026-05-10 14:58:08'),
(68, 908063, 'LOKANTA', 1, 85.00, 85.00, 'Oda 206 - muat sari', 1, 'Served', '2026-05-10 15:04:45'),
(69, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 206 - muat sari', 1, 'Served', '2026-05-10 15:04:45'),
(70, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 206 - muat sari', 1, 'Served', '2026-05-10 15:04:45'),
(71, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 103 - salad dahır', 1, 'Served', '2026-05-10 15:05:50'),
(72, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 103 - salad dahır', 1, 'Served', '2026-05-10 15:05:50'),
(73, 309080, 'LOKANTA', 1, 20.00, 20.00, 'Oda 103 - salad dahır', 1, 'Served', '2026-05-10 15:05:50'),
(74, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-05-10 15:09:42'),
(75, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-05-10 15:09:42'),
(76, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-05-10 15:09:42'),
(77, 908063, 'LOKANTA', 1, 85.00, 85.00, 'Oda 101 - asad damaer', 1, 'Served', '2026-05-10 15:17:16'),
(78, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 101 - asad damaer', 1, 'Served', '2026-05-10 15:17:16'),
(79, 908063, 'LOKANTA', 1, 85.00, 85.00, 'Oda 101 - asad damaer', 1, 'Served', '2026-05-10 15:19:43'),
(80, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 101 - asad damaer', 1, 'Served', '2026-05-10 15:19:43'),
(81, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 101 - asad damaer', 1, 'Served', '2026-05-10 15:19:43'),
(82, 908065, 'LOKANTA', 1, 0.00, 0.00, 'Oda 104 - muhubo jamac ısmaıl', 1, 'Served', '2026-05-10 15:23:12'),
(83, 908065, 'LOKANTA', 1, 75.00, 75.00, 'Oda 203 - ahmett yare farah', 1, 'Served', '2026-05-10 15:39:39'),
(84, 908063, 'LOKANTA', 1, 85.00, 85.00, 'Oda 302 - safıa satır', 1, 'Served', '2026-05-11 10:53:06'),
(85, 908064, 'LOKANTA', 1, 70.00, 70.00, 'Oda 302 - safıa satır', 1, 'Served', '2026-05-11 10:53:06'),
(86, 309080, 'LOKANTA', 1, 20.00, 20.00, 'Oda 201 - YASIN  AHMED', 1, 'Served', '2026-05-21 16:21:21'),
(87, 908064, 'LOKANTA', 1, 70.00, 70.00, 'Oda 201 - YASIN  AHMED', 1, 'Served', '2026-05-21 16:21:21'),
(88, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 202 - kamal Gaflanov', 1, 'Served', '2026-05-28 13:22:23'),
(89, 908063, 'LOKANTA', 1, 85.00, 85.00, 'Oda 202 - kamal Gaflanov', 1, 'Served', '2026-05-28 13:22:23'),
(90, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 202 - kamal Gaflanov', 1, 'Served', '2026-05-28 13:22:23'),
(91, 908062, 'LOKANTA', 1, 20.00, 20.00, 'Oda 205 - muat sari', 1, 'Served', '2026-05-31 14:37:52'),
(92, 45673, 'LOKANTA', 1, 40.00, 40.00, 'Oda 205 - muat sari', 1, 'Served', '2026-05-31 14:37:52'),
(93, 908060, 'LOKANTA', 1, 90.00, 90.00, 'Oda 205 - muat sari', 1, 'Served', '2026-05-31 14:37:52'),
(94, 908062, 'LOKANTA', 1, 20.00, 20.00, 'Masa 1 | Oda 205 - muat sari', 0, 'Served', '2026-05-31 14:38:39'),
(95, 908063, 'LOKANTA', 1, 85.00, 85.00, 'Masa 1 | Oda 205 - muat sari', 0, 'Served', '2026-05-31 14:38:39');

-- --------------------------------------------------------

--
-- Table structure for table `services`
--

CREATE TABLE `services` (
  `ServiceID` int(11) NOT NULL,
  `ReservationID` int(11) DEFAULT NULL,
  `ServiceName` varchar(100) NOT NULL,
  `ServiceDate` datetime DEFAULT current_timestamp(),
  `Cost` decimal(10,2) NOT NULL,
  `Description` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `shifts`
--

CREATE TABLE `shifts` (
  `ShiftID` int(11) NOT NULL,
  `EmployeeID` int(11) DEFAULT NULL,
  `ShiftDate` date DEFAULT NULL,
  `StartTime` time DEFAULT NULL,
  `EndTime` time DEFAULT NULL,
  `Status` varchar(20) DEFAULT 'Scheduled'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `stock_transfers`
--

CREATE TABLE `stock_transfers` (
  `TransferID` int(11) NOT NULL,
  `ProductID` int(11) NOT NULL,
  `FromLocation` varchar(50) DEFAULT NULL,
  `ToLocation` varchar(50) DEFAULT NULL,
  `Quantity` int(11) DEFAULT NULL,
  `PurchasePrice` decimal(10,2) DEFAULT 0.00,
  `EmployeeName` varchar(100) DEFAULT NULL,
  `SupplierName` varchar(100) DEFAULT NULL,
  `TransferDate` datetime DEFAULT current_timestamp(),
  `InvoiceNumber` varchar(50) DEFAULT NULL,
  `PaymentMethod` varchar(50) DEFAULT NULL,
  `Notes` varchar(200) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `stock_transfers`
--

INSERT INTO `stock_transfers` (`TransferID`, `ProductID`, `FromLocation`, `ToLocation`, `Quantity`, `PurchasePrice`, `EmployeeName`, `SupplierName`, `TransferDate`, `InvoiceNumber`, `PaymentMethod`, `Notes`) VALUES
(4, 409080, 'DEPO', 'MARKET_1', 2, 0.00, NULL, NULL, '2026-03-30 21:06:48', NULL, NULL, 'Mağazaya ürün sevkiyatı'),
(5, 201090, 'DEPO', 'MARKET_1', 1, 0.00, NULL, NULL, '2026-03-30 21:09:26', NULL, NULL, 'Mağazaya ürün sevkiyatı'),
(6, 309080, 'TEDARIKCI', 'DEPO', 5, 0.00, NULL, NULL, '2026-04-04 22:20:06', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(7, 409080, 'TEDARIKCI', 'DEPO', 5, 0.00, NULL, NULL, '2026-04-04 22:20:34', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(8, 309080, 'TEDARIKCI', 'DEPO', 1, 0.00, NULL, NULL, '2026-04-06 10:09:25', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(9, 908061, 'TEDARIKCI', 'DEPO', 5, 0.00, NULL, NULL, '2026-04-12 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(10, 908062, 'TEDARIKCI', 'DEPO', 10, 0.00, NULL, NULL, '2026-04-12 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(11, 908062, 'TEDARIKCI', 'DEPO', 5, 0.00, NULL, NULL, '2026-04-12 12:27:41', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(12, 908062, 'DEPO', 'LOKANTA', 5, 0.00, NULL, NULL, '2026-04-12 12:33:46', NULL, NULL, 'Lokanta Satış Çıkışı'),
(13, 908063, 'TEDARIKCI', 'DEPO', 12, 0.00, NULL, NULL, '2026-04-13 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(14, 908063, 'TEDARIKCI', 'DEPO', 4, 0.00, NULL, NULL, '2026-04-13 10:50:27', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(15, 908063, 'DEPO', 'LOKANTA', 7, 0.00, NULL, NULL, '2026-04-13 10:50:53', NULL, NULL, 'Lokanta Satış Çıkışı'),
(16, 908064, 'TEDARIKCI', 'DEPO', 8, 0.00, NULL, NULL, '2026-04-13 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(17, 908065, 'TEDARIKCI', 'DEPO', 8, 0.00, NULL, NULL, '2026-04-13 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(18, 45673, 'TEDARIKCI', 'DEPO', 6, 0.00, NULL, NULL, '2026-04-15 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(19, 45673, 'TEDARIKCI', 'DEPO', 5, 0.00, NULL, NULL, '2026-04-15 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(20, 45673, 'TEDARIKCI', 'DEPO', 1, 0.00, NULL, NULL, '2026-04-15 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(21, 908062, 'TEDARIKCI', 'DEPO', 1, 0.00, NULL, NULL, '2026-04-15 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(22, 908062, 'TEDARIKCI', 'DEPO', 3, 0.00, NULL, NULL, '2026-04-16 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(23, 908062, 'TEDARIKCI', 'DEPO', 1, 0.00, NULL, NULL, '2026-04-16 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(24, 908062, 'TEDARIKCI', 'DEPO', 1, 0.00, NULL, NULL, '2026-04-15 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(25, 908062, 'TEDARIKCI', 'DEPO', 1, 0.00, NULL, NULL, '2026-04-15 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(26, 309080, 'TEDARIKCI', 'DEPO', 4, 0.00, NULL, NULL, '2026-04-15 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(27, 309080, 'TEDARIKCI', 'DEPO', 1, 0.00, NULL, NULL, '2026-04-15 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(28, 908060, 'TEDARIKCI', 'DEPO', 5, 0.00, NULL, NULL, '2026-04-15 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(29, 908060, 'TEDARIKCI', 'DEPO', 3, 0.00, NULL, NULL, '2026-04-16 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(30, 908060, 'DEPO', 'LOKANTA', 3, 0.00, NULL, NULL, '2026-04-15 21:58:16', NULL, NULL, 'Lokanta Satış Çıkışı'),
(31, 908061, 'TEDARIKCI', 'DEPO', 20, 0.00, NULL, NULL, '2026-04-20 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(32, 908062, 'TEDARIKCI', 'LOKANTA', 1, 60.00, 'mahad', 'farah', '2026-04-20 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(33, 908065, 'TEDARIKCI', 'LOKANTA', 15, 80.00, 'salax mahad', 'harun', '2026-04-20 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(34, 908065, 'TEDARIKCI', 'LOKANTA', 30, 60.00, 'harun farah', 'sarhat kara', '2026-04-20 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(35, 409080, 'TEDARIKCI', 'DEPO', 30, 60.00, 'nanı', 'munahar', '2026-04-20 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(36, 908065, 'TEDARIKCI', 'DEPO', 50, 70.00, 'nanuman', 'harun salax', '2026-04-20 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(37, 908062, 'TEDARIKCI', 'DEPO', 50, 60.00, 'safar', 'samar', '2026-04-20 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(38, 908065, 'TEDARIKCI', 'DEPO', 30, 60.00, 'muhtar', 'salaman', '2026-04-20 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(39, 908062, 'TEDARIKCI', 'LOKANTA', 40, 15.00, 'mahazım', 'safarı', '2026-04-20 00:00:00', NULL, NULL, 'Stok Girişi (Mal Kabul)'),
(40, 908060, 'TEDARIKCI', 'DEPO', 10, 50.00, 'hasan', 'fardowsa', '2026-04-20 00:00:00', '', '', 'Stok Girişi (Mal Kabul)'),
(41, 45673, 'TEDARIKCI', 'DEPO', 15, 50.00, 'muhasab', 'marabler', '2026-04-20 00:00:00', '', '', 'Stok Girişi (Mal Kabul)'),
(42, 908063, 'TEDARIKCI', 'DEPO', 20, 50.00, 'fatıma', 'salman', '2026-04-20 13:06:58', '10013', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(43, 908063, 'TEDARIKCI', 'DEPO', 30, 50.00, 'aişa', 'xasan', '2026-04-20 13:06:58', '', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(44, 908065, 'TEDARIKCI', 'DEPO', 40, 50.00, 'mohan', 'safiyo', '2026-04-20 13:18:56', '10034', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(45, 908063, 'TEDARIKCI', 'LOKANTA', 60, 50.00, 'kamal', 'safar', '2026-04-20 13:24:31', '10026', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(46, 309080, 'TEDARIKCI', 'DEPO', 30, 10.00, 'can', 'bruak', '2026-04-21 09:23:05', '1000234', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(47, 309080, 'TEDARIKCI', 'LOKANTA', 20, 10.00, 'elıf acan', 'Burak han', '2026-04-21 09:24:56', '10014', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(48, 45673, 'TEDARIKCI', 'OTOMATIK', 30, 25.00, 'gamısıye', 'damır', '2026-04-21 09:56:25', '100034', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(49, 908060, 'TEDARIKCI', 'DEPO', 30, 15.00, 'anıl', 'suda', '2026-04-21 10:55:59', '10034', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(50, 908060, 'TEDARIKCI', 'LOKANTA', 30, 20.00, 'anıl', 'suda', '2026-04-21 10:55:59', '100034', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(51, 908064, 'TEDARIKCI', 'LOKANTA', 30, 30.00, 'samır', 'safır', '2026-04-21 10:57:34', '1004567', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(52, 908061, 'TEDARIKCI', 'DEPO', 40, 20.00, 'hasan', 'safir', '2026-04-27 04:16:46', '10090807', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(53, 908061, 'TEDARIKCI', 'LOKANTA', 15, 20.00, 'hasan', 'safir', '2026-04-27 04:16:46', '1009080', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(54, 908063, 'TEDARIKCI', 'LOKANTA', 30, 20.00, 'farxiyo cali', 'safir', '2026-05-04 09:34:20', '10000056', 'Kredi Kartı', 'Stok Girişi (Mal Kabul)'),
(55, 908063, 'TEDARIKCI', 'LOKANTA', 10, 20.00, 'ahmett farah', 'damir', '2026-05-04 09:36:57', '10056', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(56, 908062, 'TEDARIKCI', 'LOKANTA', 23, 25.00, 'ahmett farah', 'safir', '2026-05-04 23:13:49', '100324', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(57, 908063, 'TEDARIKCI', 'DEPO', 10, 0.00, 'ahmett farah', 'damir', '2026-05-04 23:18:31', '10034', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(58, 908064, 'TEDARIKCI', 'LOKANTA', 30, 30.00, 'farxiyo cali', 'damir', '2026-05-08 11:14:28', '100045', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(59, 201090, 'TEDARIKCI', 'DEPO', 40, 40.00, 'ahmett farah', 'safir', '2026-05-09 15:37:30', '1000078', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(60, 908062, 'TEDARIKCI', 'DEPO', 80, 10.00, 'ahmett farah', 'safir', '2026-05-09 15:38:17', '10087', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(61, 45673, 'TEDARIKCI', 'LOKANTA', 45, 20.00, 'ahmett farah', 'safir', '2026-05-10 14:56:06', '10098', 'Kredi Kartı', 'Stok Girişi (Mal Kabul)'),
(62, 908065, 'TEDARIKCI', 'LOKANTA', 40, 50.00, 'ahmett farah', 'safir', '2026-05-10 15:38:47', '13009', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(63, 45673, 'TEDARIKCI', 'LOKANTA', 90, 20.00, 'ahmett farah', 'damir', '2026-05-31 14:39:36', '1000908', 'Nakit', 'Stok Girişi (Mal Kabul)'),
(64, 201090, 'TEDARIKCI', 'DEPO', 30, 30.00, 'ahmett farah', 'safir', '2026-06-02 10:35:33', '1000045', 'Nakit', 'Stok Girişi (Mal Kabul)');

-- --------------------------------------------------------

--
-- Table structure for table `storage_stocks`
--

CREATE TABLE `storage_stocks` (
  `StorageID` int(11) NOT NULL,
  `ProductID` int(11) NOT NULL,
  `Quantity` int(11) DEFAULT 0,
  `Location` varchar(100) DEFAULT NULL,
  `ArrivalDate` datetime DEFAULT NULL,
  `LastUpdated` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `storage_stocks`
--

INSERT INTO `storage_stocks` (`StorageID`, `ProductID`, `Quantity`, `Location`, `ArrivalDate`, `LastUpdated`) VALUES
(16, 45673, 57, 'DEPO', '2026-04-21 09:56:25', '2026-04-21 09:57:20'),
(9, 201090, 70, 'DEPO', '2026-06-02 10:35:33', '2026-06-02 10:36:00'),
(7, 309080, 43, 'DEPO', '2026-04-21 09:23:05', '2026-04-21 09:24:30'),
(8, 409080, 36, 'DEPO', '2026-04-20 00:00:00', '2026-04-20 10:29:04'),
(10, 908060, 51, 'DEPO', '2026-04-21 10:55:59', '2026-04-21 10:56:43'),
(11, 908061, 65, 'DEPO', '2026-04-27 04:16:46', '2026-04-27 04:18:10'),
(12, 908062, 147, 'DEPO', '2026-05-09 15:38:17', '2026-05-09 15:39:03'),
(13, 908063, 69, 'DEPO', '2026-05-04 23:18:31', '2026-05-04 23:18:45'),
(14, 908064, 8, '', '2026-04-13 00:00:00', '2026-04-13 11:01:03'),
(15, 908065, 128, 'DEPO', '2026-04-20 13:18:56', '2026-04-20 13:19:55');

-- --------------------------------------------------------

--
-- Table structure for table `suppliers`
--

CREATE TABLE `suppliers` (
  `SupplierID` int(11) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `ContactPhone` varchar(20) DEFAULT NULL,
  `Address` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `suppliers`
--

INSERT INTO `suppliers` (`SupplierID`, `Name`, `ContactPhone`, `Address`) VALUES
(1, 'safir', NULL, NULL),
(2, 'damir', NULL, NULL),
(3, 'salad', NULL, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `UserID` int(11) NOT NULL,
  `Username` varchar(50) NOT NULL,
  `FullName` varchar(100) NOT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `Role` varchar(20) DEFAULT 'Kasiyer',
  `PhoneNumber` varchar(20) DEFAULT NULL,
  `IsActive` tinyint(1) DEFAULT 1,
  `CreatedAt` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`UserID`, `Username`, `FullName`, `Email`, `PasswordHash`, `Role`, `PhoneNumber`, `IsActive`, `CreatedAt`) VALUES
(1, 'admin', 'Sistem Yoneticisi', 'admin@pms.com', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Admin', NULL, 1, '2026-03-22 14:27:01');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `activity_log`
--
ALTER TABLE `activity_log`
  ADD PRIMARY KEY (`ActivityID`);

--
-- Indexes for table `beds`
--
ALTER TABLE `beds`
  ADD PRIMARY KEY (`BedID`),
  ADD KEY `RoomTypeID` (`RoomTypeID`);

--
-- Indexes for table `companies`
--
ALTER TABLE `companies`
  ADD PRIMARY KEY (`CompanyID`),
  ADD UNIQUE KEY `CompanyName` (`CompanyName`);

--
-- Indexes for table `customers`
--
ALTER TABLE `customers`
  ADD PRIMARY KEY (`CustomerID`),
  ADD UNIQUE KEY `IdentityNumber` (`IdentityNumber`),
  ADD KEY `UserID` (`UserID`);

--
-- Indexes for table `customer_messages`
--
ALTER TABLE `customer_messages`
  ADD PRIMARY KEY (`MessageID`),
  ADD KEY `CustomerID` (`CustomerID`);

--
-- Indexes for table `employees`
--
ALTER TABLE `employees`
  ADD PRIMARY KEY (`EmployeeID`);

--
-- Indexes for table `end_of_day_reports`
--
ALTER TABLE `end_of_day_reports`
  ADD PRIMARY KEY (`ReportID`),
  ADD UNIQUE KEY `ReportDate` (`ReportDate`);

--
-- Indexes for table `expenses`
--
ALTER TABLE `expenses`
  ADD PRIMARY KEY (`ExpenseID`);

--
-- Indexes for table `floors`
--
ALTER TABLE `floors`
  ADD PRIMARY KEY (`FloorID`),
  ADD UNIQUE KEY `FloorNumber` (`FloorNumber`);

--
-- Indexes for table `housekeeping_tasks`
--
ALTER TABLE `housekeeping_tasks`
  ADD PRIMARY KEY (`TaskID`),
  ADD KEY `RoomID` (`RoomID`);

--
-- Indexes for table `maintenance_logs`
--
ALTER TABLE `maintenance_logs`
  ADD PRIMARY KEY (`LogID`),
  ADD KEY `RoomID` (`RoomID`);

--
-- Indexes for table `manufacturers`
--
ALTER TABLE `manufacturers`
  ADD PRIMARY KEY (`ManufacturerID`),
  ADD UNIQUE KEY `Name` (`Name`);

--
-- Indexes for table `market_stocks`
--
ALTER TABLE `market_stocks`
  ADD PRIMARY KEY (`MarketStockID`),
  ADD UNIQUE KEY `ProductID` (`ProductID`,`StoreID`);

--
-- Indexes for table `payments`
--
ALTER TABLE `payments`
  ADD PRIMARY KEY (`PaymentID`),
  ADD KEY `ReservationID` (`ReservationID`);

--
-- Indexes for table `products`
--
ALTER TABLE `products`
  ADD PRIMARY KEY (`ProductID`),
  ADD UNIQUE KEY `Barcode` (`Barcode`);

--
-- Indexes for table `reservations`
--
ALTER TABLE `reservations`
  ADD PRIMARY KEY (`ReservationID`),
  ADD KEY `CustomerID` (`CustomerID`),
  ADD KEY `RoomID` (`RoomID`),
  ADD KEY `CompanyID` (`CompanyID`);

--
-- Indexes for table `restaurant_tables`
--
ALTER TABLE `restaurant_tables`
  ADD PRIMARY KEY (`TableID`),
  ADD KEY `CurrentReservationID` (`CurrentReservationID`);

--
-- Indexes for table `rooms`
--
ALTER TABLE `rooms`
  ADD PRIMARY KEY (`RoomID`),
  ADD UNIQUE KEY `RoomNumber` (`RoomNumber`),
  ADD KEY `FloorID` (`FloorID`),
  ADD KEY `RoomTypeID` (`RoomTypeID`);

--
-- Indexes for table `room_prices`
--
ALTER TABLE `room_prices`
  ADD PRIMARY KEY (`PriceID`),
  ADD KEY `RoomTypeID` (`RoomTypeID`),
  ADD KEY `idx_room_price_roomid` (`RoomID`);

--
-- Indexes for table `room_types`
--
ALTER TABLE `room_types`
  ADD PRIMARY KEY (`RoomTypeID`);

--
-- Indexes for table `sales_log`
--
ALTER TABLE `sales_log`
  ADD PRIMARY KEY (`SaleID`),
  ADD KEY `ProductID` (`ProductID`);

--
-- Indexes for table `services`
--
ALTER TABLE `services`
  ADD PRIMARY KEY (`ServiceID`),
  ADD KEY `ReservationID` (`ReservationID`);

--
-- Indexes for table `shifts`
--
ALTER TABLE `shifts`
  ADD PRIMARY KEY (`ShiftID`),
  ADD KEY `EmployeeID` (`EmployeeID`);

--
-- Indexes for table `stock_transfers`
--
ALTER TABLE `stock_transfers`
  ADD PRIMARY KEY (`TransferID`),
  ADD KEY `ProductID` (`ProductID`);

--
-- Indexes for table `storage_stocks`
--
ALTER TABLE `storage_stocks`
  ADD PRIMARY KEY (`ProductID`),
  ADD UNIQUE KEY `StorageID` (`StorageID`);

--
-- Indexes for table `suppliers`
--
ALTER TABLE `suppliers`
  ADD PRIMARY KEY (`SupplierID`),
  ADD UNIQUE KEY `Name` (`Name`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`UserID`),
  ADD UNIQUE KEY `Username` (`Username`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `activity_log`
--
ALTER TABLE `activity_log`
  MODIFY `ActivityID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=390;

--
-- AUTO_INCREMENT for table `beds`
--
ALTER TABLE `beds`
  MODIFY `BedID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `companies`
--
ALTER TABLE `companies`
  MODIFY `CompanyID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `customers`
--
ALTER TABLE `customers`
  MODIFY `CustomerID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=62;

--
-- AUTO_INCREMENT for table `customer_messages`
--
ALTER TABLE `customer_messages`
  MODIFY `MessageID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `employees`
--
ALTER TABLE `employees`
  MODIFY `EmployeeID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `end_of_day_reports`
--
ALTER TABLE `end_of_day_reports`
  MODIFY `ReportID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `expenses`
--
ALTER TABLE `expenses`
  MODIFY `ExpenseID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `floors`
--
ALTER TABLE `floors`
  MODIFY `FloorID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=1523;

--
-- AUTO_INCREMENT for table `housekeeping_tasks`
--
ALTER TABLE `housekeeping_tasks`
  MODIFY `TaskID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `maintenance_logs`
--
ALTER TABLE `maintenance_logs`
  MODIFY `LogID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `manufacturers`
--
ALTER TABLE `manufacturers`
  MODIFY `ManufacturerID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6022;

--
-- AUTO_INCREMENT for table `market_stocks`
--
ALTER TABLE `market_stocks`
  MODIFY `MarketStockID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- AUTO_INCREMENT for table `payments`
--
ALTER TABLE `payments`
  MODIFY `PaymentID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=107;

--
-- AUTO_INCREMENT for table `products`
--
ALTER TABLE `products`
  MODIFY `ProductID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=908066;

--
-- AUTO_INCREMENT for table `reservations`
--
ALTER TABLE `reservations`
  MODIFY `ReservationID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=106;

--
-- AUTO_INCREMENT for table `restaurant_tables`
--
ALTER TABLE `restaurant_tables`
  MODIFY `TableID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT for table `rooms`
--
ALTER TABLE `rooms`
  MODIFY `RoomID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14845;

--
-- AUTO_INCREMENT for table `room_prices`
--
ALTER TABLE `room_prices`
  MODIFY `PriceID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=13;

--
-- AUTO_INCREMENT for table `room_types`
--
ALTER TABLE `room_types`
  MODIFY `RoomTypeID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `sales_log`
--
ALTER TABLE `sales_log`
  MODIFY `SaleID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=96;

--
-- AUTO_INCREMENT for table `services`
--
ALTER TABLE `services`
  MODIFY `ServiceID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `shifts`
--
ALTER TABLE `shifts`
  MODIFY `ShiftID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `stock_transfers`
--
ALTER TABLE `stock_transfers`
  MODIFY `TransferID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=65;

--
-- AUTO_INCREMENT for table `storage_stocks`
--
ALTER TABLE `storage_stocks`
  MODIFY `StorageID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;

--
-- AUTO_INCREMENT for table `suppliers`
--
ALTER TABLE `suppliers`
  MODIFY `SupplierID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `UserID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=568;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `beds`
--
ALTER TABLE `beds`
  ADD CONSTRAINT `beds_ibfk_1` FOREIGN KEY (`RoomTypeID`) REFERENCES `room_types` (`RoomTypeID`);

--
-- Constraints for table `customers`
--
ALTER TABLE `customers`
  ADD CONSTRAINT `customers_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `users` (`UserID`);

--
-- Constraints for table `customer_messages`
--
ALTER TABLE `customer_messages`
  ADD CONSTRAINT `customer_messages_ibfk_1` FOREIGN KEY (`CustomerID`) REFERENCES `customers` (`CustomerID`) ON DELETE CASCADE;

--
-- Constraints for table `housekeeping_tasks`
--
ALTER TABLE `housekeeping_tasks`
  ADD CONSTRAINT `housekeeping_tasks_ibfk_1` FOREIGN KEY (`RoomID`) REFERENCES `rooms` (`RoomID`);

--
-- Constraints for table `maintenance_logs`
--
ALTER TABLE `maintenance_logs`
  ADD CONSTRAINT `maintenance_logs_ibfk_1` FOREIGN KEY (`RoomID`) REFERENCES `rooms` (`RoomID`);

--
-- Constraints for table `market_stocks`
--
ALTER TABLE `market_stocks`
  ADD CONSTRAINT `market_stocks_ibfk_1` FOREIGN KEY (`ProductID`) REFERENCES `products` (`ProductID`) ON DELETE CASCADE;

--
-- Constraints for table `payments`
--
ALTER TABLE `payments`
  ADD CONSTRAINT `payments_ibfk_1` FOREIGN KEY (`ReservationID`) REFERENCES `reservations` (`ReservationID`);

--
-- Constraints for table `reservations`
--
ALTER TABLE `reservations`
  ADD CONSTRAINT `reservations_ibfk_1` FOREIGN KEY (`CustomerID`) REFERENCES `customers` (`CustomerID`),
  ADD CONSTRAINT `reservations_ibfk_2` FOREIGN KEY (`RoomID`) REFERENCES `rooms` (`RoomID`),
  ADD CONSTRAINT `reservations_ibfk_3` FOREIGN KEY (`CompanyID`) REFERENCES `companies` (`CompanyID`);

--
-- Constraints for table `restaurant_tables`
--
ALTER TABLE `restaurant_tables`
  ADD CONSTRAINT `restaurant_tables_ibfk_1` FOREIGN KEY (`CurrentReservationID`) REFERENCES `reservations` (`ReservationID`);

--
-- Constraints for table `rooms`
--
ALTER TABLE `rooms`
  ADD CONSTRAINT `rooms_ibfk_1` FOREIGN KEY (`FloorID`) REFERENCES `floors` (`FloorID`),
  ADD CONSTRAINT `rooms_ibfk_2` FOREIGN KEY (`RoomTypeID`) REFERENCES `room_types` (`RoomTypeID`);

--
-- Constraints for table `room_prices`
--
ALTER TABLE `room_prices`
  ADD CONSTRAINT `room_prices_ibfk_1` FOREIGN KEY (`RoomTypeID`) REFERENCES `room_types` (`RoomTypeID`);

--
-- Constraints for table `sales_log`
--
ALTER TABLE `sales_log`
  ADD CONSTRAINT `sales_log_ibfk_1` FOREIGN KEY (`ProductID`) REFERENCES `products` (`ProductID`) ON DELETE CASCADE;

--
-- Constraints for table `services`
--
ALTER TABLE `services`
  ADD CONSTRAINT `services_ibfk_1` FOREIGN KEY (`ReservationID`) REFERENCES `reservations` (`ReservationID`);

--
-- Constraints for table `shifts`
--
ALTER TABLE `shifts`
  ADD CONSTRAINT `shifts_ibfk_1` FOREIGN KEY (`EmployeeID`) REFERENCES `employees` (`EmployeeID`);

--
-- Constraints for table `stock_transfers`
--
ALTER TABLE `stock_transfers`
  ADD CONSTRAINT `stock_transfers_ibfk_1` FOREIGN KEY (`ProductID`) REFERENCES `products` (`ProductID`) ON DELETE CASCADE;

--
-- Constraints for table `storage_stocks`
--
ALTER TABLE `storage_stocks`
  ADD CONSTRAINT `storage_stocks_ibfk_1` FOREIGN KEY (`ProductID`) REFERENCES `products` (`ProductID`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
