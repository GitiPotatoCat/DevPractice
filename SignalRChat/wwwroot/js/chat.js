"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();

let currentUser = '';
let typingTimeout;

// User color management
const userColors = new Map();
const colorPalette = [
    '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEEAD',
    '#D4A5A5', '#9B59B6', '#3498DB', '#E67E22', '#1ABC9C',
    '#F1C40F', '#E74C3C', '#2ECC71', '#34495E', '#16A085'
];

function getUserColor(username) {
    if (!userColors.has(username)) {
        const colorIndex = userColors.size % colorPalette.length;
        userColors.set(username, colorPalette[colorIndex]);
    }
    return userColors.get(username);
}

// DOM Elements
const joinArea = document.getElementById('joinArea');
const chatArea = document.getElementById('chatArea');
const userInput = document.getElementById('userInput');
const messageInput = document.getElementById('messageInput');
const sendButton = document.getElementById('sendButton');
const messagesList = document.getElementById('messagesList');
const messageForm = document.getElementById('messageForm');
const onlineUsersList = document.getElementById('onlineUsersList');
const typingIndicator = document.getElementById('typingIndicator');
const connectionStatus = document.getElementById('connectionStatus');

// Disable send button until connection is established
sendButton.disabled = true;

// Message receiving handler
connection.on("ReceiveMessage", function (data) {
    const { user, message, time } = data;
    const isOwnMessage = user === currentUser;
    const userColor = getUserColor(user);
    
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${isOwnMessage ? 'own' : 'other'}`;
    
    const formattedTime = new Date(time).toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit'
    });

    // Create message element with enhanced styling
    messageDiv.innerHTML = `
        <div class="message-content" style="${isOwnMessage ? `background-color: ${userColor};` : ''}">
            ${!isOwnMessage ? `
                <div class="message-user" style="color: ${userColor}">
                    <i class="bi bi-person-circle"></i>
                    ${user}
                </div>
            ` : ''}
            <div class="message-text">${formatMessage(message)}</div>
            <div class="message-time">
                <i class="bi bi-clock"></i>
                ${formattedTime}
            </div>
        </div>
    `;
    
    // Add animation class after a brief delay
    messagesList.appendChild(messageDiv);
    scrollToBottom();

    // Add hover effect for message actions
    messageDiv.addEventListener('mouseenter', () => {
        messageDiv.style.transform = 'scale(1.01)';
    });
    messageDiv.addEventListener('mouseleave', () => {
        messageDiv.style.transform = 'scale(1)';
    });
});

// Helper function to format messages
function formatMessage(message) {
    // Convert URLs to clickable links
    message = message.replace(
        /(https?:\/\/[^\s]+)/g,
        '<a href="$1" target="_blank" class="text-decoration-none">$1</a>'
    );
    
    // Convert emojis
    message = message.replace(/:\)|😊/g, '😊')
        .replace(/:\(|😢/g, '😢')
        .replace(/:D|😃/g, '😃')
        .replace(/;\)|😉/g, '😉')
        .replace(/:P|😛/g, '😛');
    
    return message;
}

// Online users handling with enhanced styling
connection.on("UpdateOnlineUsers", function (users) {
    onlineUsersList.innerHTML = '';
    users.forEach(user => {
        const userColor = getUserColor(user);
        const userDiv = document.createElement('div');
        userDiv.className = 'user-item';
        userDiv.innerHTML = `
            <span class="user-status online"></span>
            <div class="d-flex flex-column">
                <span style="color: ${userColor}">
                    <i class="bi bi-person-circle me-1"></i>
                    ${user}
                </span>
                <small class="text-muted" style="font-size: 0.75rem;">
                    <i class="bi bi-circle-fill text-success" style="font-size: 0.5rem;"></i>
                    Online
                </small>
            </div>
        `;
        onlineUsersList.appendChild(userDiv);
    });
});

// Typing indicator handling with enhanced styling
connection.on("UserTyping", function (user, isTyping) {
    if (isTyping) {
        const userColor = getUserColor(user);
        typingIndicator.innerHTML = `
            <em>
                <span style="color: ${userColor};">${user}</span> is typing
                <div class="typing-dots">
                    <span></span>
                    <span></span>
                    <span></span>
                </div>
            </em>
        `;
    }
    typingIndicator.classList.toggle('d-none', !isTyping);
});

// Connection handling
connection.onreconnecting(() => {
    connectionStatus.textContent = 'Reconnecting...';
    connectionStatus.className = 'badge bg-warning';
    sendButton.disabled = true;
});

connection.onreconnected(() => {
    connectionStatus.textContent = 'Connected';
    connectionStatus.className = 'badge bg-success';
    sendButton.disabled = false;
});

connection.onclose(() => {
    connectionStatus.textContent = 'Disconnected';
    connectionStatus.className = 'badge bg-danger';
    sendButton.disabled = true;
});

// Join chat handling
document.getElementById('joinButton').addEventListener('click', function() {
    const username = userInput.value.trim();
    if (username) {
        currentUser = username;
        connection.start()
            .then(() => connection.invoke("JoinChat", username))
            .then(() => {
                joinArea.classList.add('d-none');
                chatArea.classList.remove('d-none');
                sendButton.disabled = false;
            })
            .catch(err => console.error(err));
    }
});

// Message form handling
messageForm.addEventListener('submit', function (event) {
    event.preventDefault();
    const message = messageInput.value.trim();
    if (message && currentUser) {
        connection.invoke("SendMessage", currentUser, message)
            .catch(err => console.error(err));
        messageInput.value = '';
    }
});

// Typing indicator handling
messageInput.addEventListener('input', function() {
    if (currentUser) {
        connection.invoke("SendTypingIndicator", currentUser, true);
        
        clearTimeout(typingTimeout);
        typingTimeout = setTimeout(() => {
            connection.invoke("SendTypingIndicator", currentUser, false);
        }, 1000);
    }
});

// Utility function to scroll to bottom of messages
function scrollToBottom() {
    const container = document.getElementById('messagesContainer');
    container.scrollTop = container.scrollHeight;
}