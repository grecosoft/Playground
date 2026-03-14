#!/bin/bash

# ============================================
# CONFIGURATION - Replace with your values
# ============================================
CLIENT_ID="a510640d-98d2-4039-bc5c-dfef0bac2501"  # Your MyAppClient ID
TENANT_ID="d105c533-f553-4e8c-b2a5-e5bdf9a67cf1"  # Your Tenant ID
API_CLIENT_ID="b3eaaaf6-a4a0-45a0-9e49-6febc071e71a"  # Your MyWebAPI ID
API_SCOPE="api://${API_CLIENT_ID}/.default"

# ============================================
# Step 1: Initiate Device Code Flow
# ============================================
echo "Initiating device code flow..."

DEVICE_CODE_RESPONSE=$(curl -s -X POST \
  "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/devicecode" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=${CLIENT_ID}&scope=${API_SCOPE}")

# Check for errors
ERROR=$(echo $DEVICE_CODE_RESPONSE | jq -r '.error // empty')
if [ ! -z "$ERROR" ]; then
    echo "Error initiating device code flow:"
    echo $DEVICE_CODE_RESPONSE | jq -r '.error_description'
    exit 1
fi

# Extract values
DEVICE_CODE=$(echo $DEVICE_CODE_RESPONSE | jq -r '.device_code')
USER_CODE=$(echo $DEVICE_CODE_RESPONSE | jq -r '.user_code')
VERIFICATION_URL=$(echo $DEVICE_CODE_RESPONSE | jq -r '.verification_uri')
EXPIRES_IN=$(echo $DEVICE_CODE_RESPONSE | jq -r '.expires_in')
INTERVAL=$(echo $DEVICE_CODE_RESPONSE | jq -r '.interval // 5')

# ============================================
# Step 2: Display Instructions to User
# ============================================
echo ""
echo "============================================"
echo "  AUTHENTICATION REQUIRED"
echo "============================================"
echo ""
echo "1. Open this URL in a browser:"
echo "   ${VERIFICATION_URL}"
echo ""
echo "2. Enter this code:"
echo ""
echo "   ${USER_CODE}"
echo ""
echo "============================================"
echo ""
echo "Waiting for you to authenticate..."
echo "(This will expire in ${EXPIRES_IN} seconds)"
echo ""

# ============================================
# Step 3: Poll for Token
# ============================================
MAX_ATTEMPTS=$((EXPIRES_IN / INTERVAL))
ATTEMPT=0

while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
  sleep $INTERVAL
  
  TOKEN_RESPONSE=$(curl -s -X POST \
    "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "client_id=${CLIENT_ID}&grant_type=urn:ietf:params:oauth:grant-type:device_code&device_code=${DEVICE_CODE}")
  
  ERROR=$(echo $TOKEN_RESPONSE | jq -r '.error // empty')
  
  if [ "$ERROR" == "authorization_pending" ]; then
    # Still waiting
    echo -n "."
    ATTEMPT=$((ATTEMPT + 1))
  elif [ ! -z "$ERROR" ]; then
    # Error occurred
    echo ""
    echo "Error: $ERROR"
    echo $TOKEN_RESPONSE | jq -r '.error_description'
    exit 1
  else
    # Success!
    echo ""
    echo ""
    echo "============================================"
    echo "  ✓ AUTHENTICATION SUCCESSFUL"
    echo "============================================"
    
    ACCESS_TOKEN=$(echo $TOKEN_RESPONSE | jq -r '.access_token')
    REFRESH_TOKEN=$(echo $TOKEN_RESPONSE | jq -r '.refresh_token // empty')
    EXPIRES_IN=$(echo $TOKEN_RESPONSE | jq -r '.expires_in')
    
    # Save tokens
    echo $ACCESS_TOKEN > token.txt
    echo "✓ Access token saved to: token.txt"
    
    if [ ! -z "$REFRESH_TOKEN" ]; then
        echo $REFRESH_TOKEN > refresh_token.txt
        echo "✓ Refresh token saved to: refresh_token.txt"
    fi
    
    # Decode token to show roles
    echo ""
    echo "============================================"
    echo "  TOKEN INFORMATION"
    echo "============================================"
    
    # Extract and decode payload
    PAYLOAD=$(echo $ACCESS_TOKEN | cut -d '.' -f 2)
    
    # Add padding if needed (base64 requires length to be multiple of 4)
    MOD=$((${#PAYLOAD} % 4))
    if [ $MOD -eq 2 ]; then
        PAYLOAD="${PAYLOAD}=="
    elif [ $MOD -eq 3 ]; then
        PAYLOAD="${PAYLOAD}="
    fi
    
    # Decode
    DECODED=$(echo $PAYLOAD | base64 -d 2>/dev/null)
    
    # Extract information
    NAME=$(echo $DECODED | jq -r '.name // "N/A"')
    EMAIL=$(echo $DECODED | jq -r '.preferred_username // "N/A"')
    ROLES=$(echo $DECODED | jq -r '.roles // [] | join(", ")')
    EXP=$(echo $DECODED | jq -r '.exp')
    AUD=$(echo $DECODED | jq -r '.aud')
    
    # Convert expiration to human-readable
    if command -v date &> /dev/null; then
        EXPIRY=$(date -d @$EXP 2>/dev/null || date -r $EXP 2>/dev/null)
    else
        EXPIRY="$EXP (Unix timestamp)"
    fi
    
    echo ""
    echo "User: $NAME"
    echo "Email: $EMAIL"
    echo "Audience: $AUD"
    echo ""
    echo "Roles:"
    if [ ! -z "$ROLES" ] && [ "$ROLES" != "null" ] && [ "$ROLES" != "" ]; then
        echo $DECODED | jq -r '.roles[] | "  - " + .'
    else
        echo "  ⚠ No roles found!"
        echo "  Check: Enterprise Applications → MyWebAPI → Users and groups"
    fi
    echo ""
    echo "Token expires: $EXPIRY"
    echo "Expires in: ${EXPIRES_IN} seconds"
    echo ""
    echo "============================================"
    echo ""
    echo "Full token payload:"
    echo $DECODED | jq '.'
    echo ""
    
    exit 0
  fi
done

echo ""
echo "Timeout: Authentication not completed within ${EXPIRES_IN} seconds"
exit 1
