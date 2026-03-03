#!/bin/bash

# for dir in ./service-one/k8s/envs/*; do
#     echo "Processing: $dir"
#     (
#         env_name=$(basename "$dir")
#         echo "Running kustomize on Environment: ${env_name} located at: ${dir}"
#         cd $dir
        
#         kubectl kustomize . > "${env_name}-resources.yaml"
#     )
# done


for dir in ./solution/src/service-one/k8s/envs/*; do
    echo "Processing: $dir"
    (
        env_name=$(basename "$dir")
        echo "Running kustomize on Environment: ${env_name} located at: ${dir}"
        # cd $dir
        
        # kubectl kustomize . > "${env_name}-resources.yaml"
    )
done

# mkdir -p "./solution/artifacts"
# ARTIFACTS_DIR="./solution/artifacts"
for dir in ./solution/src/service-one/k8s/envs/*; do
     echo "Processing: $dir"
     (
         env_name=$(basename "$dir")
         echo "Running kustomize on Environment: ${env_name} located at: ${dir}"
#         cd $dir
        
#         kubectl kustomize . > "${ARTIFACTS_DIR}/${env_name}-resources.yaml"
     )
done
