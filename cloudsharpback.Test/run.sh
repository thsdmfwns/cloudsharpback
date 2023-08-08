docker build .. --file ./Dockerfile -t cs_test
docker run --network cloudsharp --name cs_test --rm cs_test