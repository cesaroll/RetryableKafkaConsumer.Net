#!/bin/bash

# Validate message parameter
if [ -z "$1" ]; then
  echo "Usage: $0 <message> [number_of_parallel_instances]"
  exit 1
fi

message="$1"
num_instances=${2:-1}  # Default to 1 instance if not provided

# Function to handle cleanup on Ctrl+C
cleanup() {
  echo "Stopping all background processes..."
  kill 0  # Kills all child processes in the same process group
  exit 0
}

# Trap SIGINT (Ctrl+C) and call cleanup function
trap cleanup SIGINT

# Run the script X times in parallel
for ((i=1; i<=num_instances; i++)); do
  (
    counter=1
    while true; do
      
      # Display log only every 10 iterations
      if (( counter % 10 == 0 )); then
        echo "Instance $i - Counter $counter"
      fi
      
      key="I${i}C${counter}"  # Format: I<instance_number>C<counter>
      
      echo "$key:{\"Key\":\"$key\",\"Value\":\"$message\"}" | kcat -P -b localhost:9092 -t test-topic -K:
      ((counter++))
      # Uncomment sleep to slow down message production if needed
      # sleep 0.1
    done
  ) &
done

# Wait for all background jobs to finish
wait